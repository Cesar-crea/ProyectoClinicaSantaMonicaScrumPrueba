using ProyectoSantaMonica_Cesar.Models;
using ProyectoSantaMonica_Cesar.Models.enums;
using ProyectoSantaMonica_Cesar.Repository;

namespace ProyectoSantaMonica_Cesar.Service.CitaService
{
    public class CitaService : ICitaService
    {
        private readonly CitaRepository _repo;

        public CitaService(CitaRepository repo)
        {
            _repo = repo;
        }

        // =========================================================
        // LISTAR
        // =========================================================

        public async Task<List<Cita>> ListarAsync()
        {
            await _repo.ActualizarEstadosAutomaticosAsync();

            return await _repo.ListarAsync();
        }

        // =========================================================
        // BUSCAR POR PACIENTE
        // =========================================================

        public async Task<List<Cita>> BuscarAsync(
     string texto)
     => await _repo.BuscarAsync(texto);

        // =========================================================
        // REGISTRAR
        // =========================================================

        public async Task<string> RegistrarAsync(Cita c)
        {
            Validar(c);

            return await _repo.RegistrarAsync(c);
        }

        // =========================================================
        // ACTUALIZAR
        // =========================================================

        public async Task<string> ActualizarAsync(Cita c)
        {
            Validar(c);

            return await _repo.ActualizarAsync(c);
        }

        // =========================================================
        // ELIMINAR
        // =========================================================

        public async Task<string> EliminarAsync(int id)
            => await _repo.EliminarAsync(id);

        // =========================================================
        // OBTENER POR ID
        // =========================================================

        public async Task<Cita?> ObtenerPorIdAsync(int id)
            => await _repo.ObtenerPorIdAsync(id);

        // =========================================================
        // HORARIOS OCUPADOS
        // =========================================================

        public async Task<List<Cita>> HorariosOcupadosAsync(
            int idMedico,
            DateTime fecha)
        {
            return await _repo.HorariosOcupadosAsync(
                idMedico,
                fecha);
        }

        // =========================================================
        // LISTAR POR ESTADO
        // =========================================================

        public async Task<List<Cita>> ListarPorEstadoAsync(
            EstadoCita estado)
            => await _repo.ListarPorEstadoAsync(
                estado.ToString());

        // =========================================================
        // PENDIENTES POR PACIENTE
        // =========================================================

        public async Task<List<Cita>> PendientesPorPacienteAsync(
            string dato)
            => await _repo.PendientesPorPacienteAsync(dato);

        // =========================================================
        // ACTUALIZAR ESTADO
        // =========================================================

        public async Task<string> ActualizarEstadoAsync(
            int id,
            EstadoCita estado)
            => await _repo.ActualizarEstadoAsync(
                id,
                estado);

        // =========================================================
        // ACTUALIZAR ESTADOS AUTOMÁTICOS
        // =========================================================

        public async Task ActualizarEstadosAutomaticosAsync()
            => await _repo.ActualizarEstadosAutomaticosAsync();

        // =========================================================
        // VALIDACIÓN BÁSICA
        // =========================================================

        private void Validar(Cita c)
        {
            if (c.Fecha < DateTime.Now.Date)
            {
                throw new Exception("Fecha inválida");
            }

            if (c.Hora == default)
            {
                throw new Exception("Hora inválida");
            }
        }

        // =========================================================
        // VALIDAR SI MÉDICO ATIENDE
        // =========================================================

        public async Task<bool> MedicoAtiendeEnHorarioAsync(
            int idMedico,
            DateTime fecha,
            TimeSpan hora)
        {
            return await _repo.MedicoAtiendeEnHorarioAsync(
                idMedico,
                fecha,
                hora);
        }

        // =========================================================
        // VALIDACIÓN COMPLETA DEL HORARIO
        // =========================================================

        public async Task<string?> ValidarHorarioCitaAsync(
            int idMedico,
            DateTime fecha,
            TimeSpan hora,
            int idCitaExcluir = 0)
        {
            return await _repo.ValidarHorarioCitaAsync(
                idMedico,
                fecha,
                hora,
                idCitaExcluir);
        }
    }
}