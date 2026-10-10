using ProyectoSantaMonica_Cesar.Models.ViewModels;

namespace ProyectoSantaMonica_Cesar.Service.ReporteService
{
    public interface IReporteService
    {
        Task<List<ReporteCitaRow>> ObtenerCitasAsync(ReporteFiltrosViewModel filtros);
        Task<List<ReporteCmpRow>> ObtenerCmpAsync();

        Task<List<ReporteComprobantePagoRow>> ObtenerComprobantesAsync(
ReporteComprobantePagoFiltrosViewModel filtros);
        byte[] GenerarCitasExcel(IReadOnlyCollection<ReporteCitaRow> rows, ReporteFiltrosViewModel filtros);
        byte[] GenerarCmpExcel(IReadOnlyCollection<ReporteCmpRow> rows);
        byte[] GenerarCitasPdf(IReadOnlyCollection<ReporteCitaRow> rows, ReporteFiltrosViewModel filtros);
        byte[] GenerarCmpPdf(IReadOnlyCollection<ReporteCmpRow> rows);

        byte[] GenerarHistorialExcel(
    IReadOnlyCollection<ProyectoSantaMonica_Cesar.Models.HistorialUsuario> rows);

        byte[] GenerarHistorialPdf(
            IReadOnlyCollection<ProyectoSantaMonica_Cesar.Models.HistorialUsuario> rows);

        byte[] GenerarComprobantesExcel(
            IReadOnlyCollection<ReporteComprobantePagoRow> rows);

        byte[] GenerarComprobantesPdf(
            IReadOnlyCollection<ReporteComprobantePagoRow> rows);

    }
}
