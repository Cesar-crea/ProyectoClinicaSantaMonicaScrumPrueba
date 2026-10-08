using Microsoft.Data.SqlClient;
using ProyectoSantaMonica_Cesar.Models;
using ProyectoSantaMonica_Cesar.Models.enums;
using System.Data;

namespace ProyectoSantaMonica_Cesar.Repository
{
    public class CitaRepository
    {
        private readonly string _connection;

        public CitaRepository(IConfiguration configuration)
        {
            _connection = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException(
                    "No se encontró la cadena de conexión 'DefaultConnection'.");
        }

        // =========================================================
        // MAPEO BASE
        // =========================================================

        private Cita MapearBase(SqlDataReader dr)
        {
            return new Cita
            {
                Id_Cita = dr.GetInt32(0),
                Id_Medico = dr.GetInt32(1),
                Id_Paciente = dr.GetInt32(2),
                Id_Usuario = dr.GetInt64(3),
                Fecha = dr.GetDateTime(4),
                Hora = dr.GetTimeSpan(5),
                Motivo = dr.IsDBNull(6) ? "" : dr.GetString(6),
                Estado = Enum.Parse<EstadoCita>(dr.GetString(7))
            };
        }

        // =========================================================
        // MAPEO COMPLETO
        // =========================================================

        private Cita MapearCompleto(SqlDataReader dr)
        {
            return new Cita
            {
                Id_Cita = dr.GetInt32(0),
                Id_Medico = dr.GetInt32(1),
                Id_Paciente = dr.GetInt32(2),
                Id_Usuario = dr.GetInt64(3),
                Fecha = dr.GetDateTime(4),
                Hora = dr.GetTimeSpan(5),
                Motivo = dr.IsDBNull(6) ? "" : dr.GetString(6),
                Estado = Enum.Parse<EstadoCita>(dr.GetString(7)),

                Paciente = new Paciente
                {
                    Nombres = ExisteColumna(dr, "PacienteNombre")
                        ? dr["PacienteNombre"]?.ToString()
                        : "",

                    Apellidos = ExisteColumna(dr, "PacienteApellido")
                        ? dr["PacienteApellido"]?.ToString()
                        : "",

                    Dni = ExisteColumna(dr, "PacienteDni")
                        ? dr["PacienteDni"]?.ToString()
                        : ""
                },

                Medico = new Medico
                {
                    Nombres = ExisteColumna(dr, "MedicoNombre")
                        ? dr["MedicoNombre"]?.ToString()
                        : "",

                    Apellidos = ExisteColumna(dr, "MedicoApellido")
                        ? dr["MedicoApellido"]?.ToString()
                        : "",

                    Especialidad = ExisteColumna(dr, "Especialidad")
                        ? dr["Especialidad"]?.ToString()
                        : ""
                },

                Usuario = new Usuario
                {
                    Nombres = ExisteColumna(dr, "UsuarioNombre")
                        ? dr["UsuarioNombre"]?.ToString()
                        : "",

                    Apellidos = ExisteColumna(dr, "UsuarioApellido")
                        ? dr["UsuarioApellido"]?.ToString()
                        : ""
                }
            };
        }

        // =========================================================
        // LISTAR
        // =========================================================

        public async Task<List<Cita>> ListarAsync()
        {
            var lista = new List<Cita>();

            using var cn = new SqlConnection(_connection);
            using var cmd = new SqlCommand("sp_listarCitas", cn);

            cmd.CommandType = CommandType.StoredProcedure;

            await cn.OpenAsync();

            using var dr = await cmd.ExecuteReaderAsync();

            while (await dr.ReadAsync())
            {
                lista.Add(MapearCompleto(dr));
            }

            return lista;
        }

        // =========================================================
        // BUSCAR POR PACIENTE
        // =========================================================

        public async Task<List<Cita>> BuscarPorPacienteAsync(string texto)
        {
            var lista = new List<Cita>();

            using var cn = new SqlConnection(_connection);
            using var cmd = new SqlCommand("sp_buscarCitasPaciente", cn);

            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("@Texto", texto);

            await cn.OpenAsync();

            using var dr = await cmd.ExecuteReaderAsync();

            while (await dr.ReadAsync())
            {
                lista.Add(MapearCompleto(dr));
            }

            return lista;
        }

        // =========================================================
        // MEDICO ATIENDE EN HORARIO
        // =========================================================
        //
        // Comprueba:
        // 1. Que el médico tenga horario ese día.
        // 2. Que la hora esté dentro de su horario.
        //
        // =========================================================

        public async Task<bool> MedicoAtiendeEnHorarioAsync(
            int idMedico,
            DateTime fecha,
            TimeSpan hora)
        {
            fecha = fecha.Date;

            if (idMedico <= 0)
            {
                return false;
            }

            // Utilizamos el mismo helper utilizado por el Controller
            // para obtener el día de la semana del proyecto.
            var diaSemana = DIaSemanaHelper.FromDateTime(fecha);

            const string sql = @"
                SELECT COUNT(*)
                FROM Horario
                WHERE Id_Medico = @Id_Medico
                  AND Dia_Semana = @Dia_Semana
                  AND @Hora >= Horario_Entrada
                  AND @Hora < Horario_Salida;";

            using var cn = new SqlConnection(_connection);
            using var cmd = new SqlCommand(sql, cn);

            cmd.Parameters.AddWithValue(
                "@Id_Medico",
                idMedico);

            cmd.Parameters.AddWithValue(
                "@Dia_Semana",
                Convert.ToInt32(diaSemana));

            cmd.Parameters.AddWithValue(
                "@Hora",
                hora);

            await cn.OpenAsync();

            var resultado = Convert.ToInt32(
                await cmd.ExecuteScalarAsync());

            return resultado > 0;
        }

        // =========================================================
        // VALIDAR HORARIO DE CITA
        // =========================================================
        //
        // Se utiliza tanto al registrar como al editar.
        //
        // idCitaExcluir:
        //      0 = registrar nueva cita
        //      >0 = editar cita existente
        //
        // Al editar, la propia cita NO cuenta como ocupada.
        //
        // =========================================================

        public async Task<string?> ValidarHorarioCitaAsync(
            int idMedico,
            DateTime fecha,
            TimeSpan hora,
            int idCitaExcluir = 0)
        {
            fecha = fecha.Date;

            // =====================================================
            // 1. VALIDAR MÉDICO
            // =====================================================

            if (idMedico <= 0)
            {
                return "MEDICO_INVALIDO";
            }

            // =====================================================
            // 2. VALIDAR FECHA
            // =====================================================

            if (fecha < DateTime.Today)
            {
                return "FECHA_PASADA";
            }

            // =====================================================
            // 3. VALIDAR HORA
            // =====================================================

            if (hora == TimeSpan.Zero)
            {
                return "HORA_INVALIDA";
            }

            // =====================================================
            // 4. SI ES HOY, LA HORA DEBE SER FUTURA
            // =====================================================

            if (fecha == DateTime.Today &&
                hora <= DateTime.Now.TimeOfDay)
            {
                return "HORA_YA_PASO";
            }

            // =====================================================
            // 5. VALIDAR HORARIO DEL MÉDICO
            // =====================================================

            bool atiende = await MedicoAtiendeEnHorarioAsync(
                idMedico,
                fecha,
                hora);

            if (!atiende)
            {
                return "HORA_FUERA_HORARIO";
            }

            // =====================================================
            // 6. VALIDAR HORA OCUPADA
            // =====================================================
            //
            // Si idCitaExcluir > 0:
            // se excluye esa misma cita.
            //
            // Esto permite editar una cita y mantener su misma hora.
            //
            // =====================================================

            const string sqlCita = @"
                SELECT COUNT(*)
                FROM Cita
                WHERE Id_Medico = @Id_Medico
                  AND CAST(Fecha AS date) = @Fecha
                  AND Hora = @Hora
                  AND Estado <> 'CANCELADO'
                  AND (
                        @Id_CitaExcluir = 0
                        OR Id_Cita <> @Id_CitaExcluir
                      );";

            using var connection = new SqlConnection(_connection);

            await connection.OpenAsync();

            using var cmdCita = new SqlCommand(
                sqlCita,
                connection);

            cmdCita.Parameters.AddWithValue(
                "@Id_Medico",
                idMedico);

            cmdCita.Parameters.AddWithValue(
                "@Fecha",
                fecha);

            cmdCita.Parameters.AddWithValue(
                "@Hora",
                hora);

            cmdCita.Parameters.AddWithValue(
                "@Id_CitaExcluir",
                idCitaExcluir);

            var resultado = Convert.ToInt32(
                await cmdCita.ExecuteScalarAsync());

            if (resultado > 0)
            {
                return "HORA_OCUPADA";
            }

            // =====================================================
            // 7. TODO CORRECTO
            // =====================================================

            return null;
        }

        // =========================================================
        // INSERTAR
        // =========================================================

        public async Task<string> RegistrarAsync(Cita c)
        {
            using var cn = new SqlConnection(_connection);
            using var cmd = new SqlCommand("sp_insertarCita", cn);

            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue(
                "@Id_Medico",
                c.Id_Medico);

            cmd.Parameters.AddWithValue(
                "@Id_Paciente",
                c.Id_Paciente);

            cmd.Parameters.AddWithValue(
                "@Id_Usuario",
                c.Id_Usuario);

            cmd.Parameters.AddWithValue(
                "@Fecha",
                c.Fecha);

            cmd.Parameters.AddWithValue(
                "@Hora",
                c.Hora);

            cmd.Parameters.AddWithValue(
                "@Motivo",
                (object?)c.Motivo ?? DBNull.Value);

            await cn.OpenAsync();

            return (await cmd.ExecuteScalarAsync())?.ToString()
                   ?? "Error";
        }

        // =========================================================
        // ACTUALIZAR
        // =========================================================

        public async Task<string> ActualizarAsync(Cita c)
        {
            using var cn = new SqlConnection(_connection);
            using var cmd = new SqlCommand(
                "sp_actualizarCita",
                cn);

            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue(
                "@Id_Cita",
                c.Id_Cita);

            cmd.Parameters.AddWithValue(
                "@Fecha",
                c.Fecha);

            cmd.Parameters.AddWithValue(
                "@Hora",
                c.Hora);

            cmd.Parameters.AddWithValue(
                "@Motivo",
                (object?)c.Motivo ?? DBNull.Value);

            await cn.OpenAsync();

            return (await cmd.ExecuteScalarAsync())?.ToString()
                   ?? "Error";
        }

        // =========================================================
        // ELIMINAR
        // =========================================================

        public async Task<string> EliminarAsync(int id)
        {
            using var cn = new SqlConnection(_connection);
            using var cmd = new SqlCommand(
                "sp_eliminarCita",
                cn);

            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue(
                "@Id_Cita",
                id);

            await cn.OpenAsync();

            return (await cmd.ExecuteScalarAsync())?.ToString()
                   ?? "Error";
        }

        // =========================================================
        // OBTENER POR ID
        // =========================================================

        public async Task<Cita?> ObtenerPorIdAsync(int id)
        {
            using var cn = new SqlConnection(_connection);
            using var cmd = new SqlCommand(
                "sp_obtenerCitaPorId",
                cn);

            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue(
                "@Id_Cita",
                id);

            await cn.OpenAsync();

            using var dr = await cmd.ExecuteReaderAsync();

            if (await dr.ReadAsync())
            {
                return MapearCompleto(dr);
            }

            return null;
        }

        // =========================================================
        // HORARIOS OCUPADOS
        // =========================================================

        public async Task<List<Cita>> HorariosOcupadosAsync(
            int idMedico,
            DateTime fecha)
        {
            var lista = new List<Cita>();

            using var cn = new SqlConnection(_connection);
            using var cmd = new SqlCommand(
                "sp_horariosOcupados",
                cn);

            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue(
                "@Id_Medico",
                idMedico);

            cmd.Parameters.AddWithValue(
                "@Fecha",
                fecha);

            await cn.OpenAsync();

            using var dr = await cmd.ExecuteReaderAsync();

            while (await dr.ReadAsync())
            {
                lista.Add(new Cita
                {
                    Hora = dr["Hora"] != DBNull.Value
                        ? (TimeSpan)dr["Hora"]
                        : TimeSpan.Zero,

                    Motivo = dr["Motivo"]?.ToString(),

                    Paciente = new Paciente
                    {
                        Nombres = dr["Nombres"]?.ToString(),
                        Apellidos = dr["Apellidos"]?.ToString()
                    }
                });
            }

            return lista;
        }

        // =========================================================
        // LISTAR POR ESTADO
        // =========================================================

        public async Task<List<Cita>> ListarPorEstadoAsync(
            string estado)
        {
            var lista = new List<Cita>();

            using var cn = new SqlConnection(_connection);
            using var cmd = new SqlCommand(
                "sp_listarCitasPorEstado",
                cn);

            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue(
                "@Estado",
                estado);

            await cn.OpenAsync();

            using var dr = await cmd.ExecuteReaderAsync();

            while (await dr.ReadAsync())
            {
                lista.Add(MapearCompleto(dr));
            }

            return lista;
        }

        // =========================================================
        // PENDIENTES POR PACIENTE
        // =========================================================

        public async Task<List<Cita>> PendientesPorPacienteAsync(
            string dato)
        {
            var lista = new List<Cita>();

            using var cn = new SqlConnection(_connection);
            using var cmd = new SqlCommand(
                "sp_buscarPendientesPaciente",
                cn);

            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue(
                "@Dato",
                dato);

            await cn.OpenAsync();

            using var dr = await cmd.ExecuteReaderAsync();

            while (await dr.ReadAsync())
            {
                lista.Add(MapearCompleto(dr));
            }

            return lista;
        }

        // =========================================================
        // ACTUALIZAR ESTADO
        // =========================================================

        public async Task<string> ActualizarEstadoAsync(
            int idCita,
            EstadoCita estado)
        {
            using var cn = new SqlConnection(_connection);
            using var cmd = new SqlCommand(
                "sp_actualizarEstadoCita",
                cn);

            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue(
                "@Id_Cita",
                idCita);

            cmd.Parameters.AddWithValue(
                "@Estado",
                estado.ToString());

            await cn.OpenAsync();

            var result = await cmd.ExecuteScalarAsync();

            return result?.ToString() ?? "Error";
        }

        // =========================================================
        // ACTUALIZAR ESTADOS AUTOMÁTICOS
        // =========================================================

        public async Task ActualizarEstadosAutomaticosAsync()
        {
            using var cn = new SqlConnection(_connection);
            using var cmd = new SqlCommand(
                "sp_actualizarEstadosCitasCompleto",
                cn);

            cmd.CommandType = CommandType.StoredProcedure;

            await cn.OpenAsync();

            await cmd.ExecuteNonQueryAsync();
        }

        // =========================================================
        // VERIFICAR SI EXISTE COLUMNA
        // =========================================================

        private bool ExisteColumna(
            SqlDataReader dr,
            string columna)
        {
            for (int i = 0; i < dr.FieldCount; i++)
            {
                if (dr.GetName(i).Equals(
                    columna,
                    StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }
    }
}