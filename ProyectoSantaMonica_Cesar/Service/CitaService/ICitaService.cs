using ProyectoSantaMonica_Cesar.Models;
using ProyectoSantaMonica_Cesar.Models.enums;

namespace ProyectoSantaMonica_Cesar.Service.CitaService
{
    public interface ICitaService
    {
        // =========================================================
        // CITAS
        // =========================================================

        Task<List<Cita>> ListarAsync();

        Task<List<Cita>> BuscarPorPacienteAsync(
            string texto);

        Task<string> RegistrarAsync(
            Cita cita);

        Task<string> ActualizarAsync(
            Cita cita);

        Task<string> EliminarAsync(
            int id);

        Task<Cita?> ObtenerPorIdAsync(
            int id);

        // =========================================================
        // HORARIOS
        // =========================================================

        Task<List<Cita>> HorariosOcupadosAsync(
            int idMedico,
            DateTime fecha);

        Task<bool> MedicoAtiendeEnHorarioAsync(
            int idMedico,
            DateTime fecha,
            TimeSpan hora);

        Task<string?> ValidarHorarioCitaAsync(
            int idMedico,
            DateTime fecha,
            TimeSpan hora,
            int idCitaExcluir = 0);

        // =========================================================
        // ESTADOS
        // =========================================================

        Task<List<Cita>> ListarPorEstadoAsync(
            EstadoCita estado);

        Task<List<Cita>> PendientesPorPacienteAsync(
            string dato);

        Task<string> ActualizarEstadoAsync(
            int id,
            EstadoCita estado);

        Task ActualizarEstadosAutomaticosAsync();
    }
}