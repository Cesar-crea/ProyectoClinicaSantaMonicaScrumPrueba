using ProyectoSantaMonica_Cesar.Models.ViewModels;

namespace ProyectoSantaMonica_Cesar.Service.ReporteService
{
    public interface IReporteService
    {
        Task<List<ReporteCitaRow>> ObtenerCitasAsync(ReporteFiltrosViewModel filtros);
        Task<List<ReporteCmpRow>> ObtenerCmpAsync();
        byte[] GenerarCitasExcel(IReadOnlyCollection<ReporteCitaRow> rows, ReporteFiltrosViewModel filtros);
        byte[] GenerarCmpExcel(IReadOnlyCollection<ReporteCmpRow> rows);
        byte[] GenerarCitasPdf(IReadOnlyCollection<ReporteCitaRow> rows, ReporteFiltrosViewModel filtros);
        byte[] GenerarCmpPdf(IReadOnlyCollection<ReporteCmpRow> rows);
    }
}
