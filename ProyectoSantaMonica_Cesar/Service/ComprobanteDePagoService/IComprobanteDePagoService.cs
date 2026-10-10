using ProyectoSantaMonica_Cesar.Models;

namespace ProyectoSantaMonica_Cesar.Service.ComprobanteDePagoService
{
    public interface IComprobanteDePagoService
    {
        Task GuardarAsync(ReporteComprobantePagoRow c);
        Task<List<ReporteComprobantePagoRow>> ListarAsync();
        Task<List<ReporteComprobantePagoRow>> BuscarAsync(string dato);
        Task AnularAsync(int id);
        Task<ReporteComprobantePagoRow?> BuscarPorIdAsync(int id);

        Task<List<ReporteComprobantePagoRow>> BuscarPorPacienteAsync(string dato);
    }
}
