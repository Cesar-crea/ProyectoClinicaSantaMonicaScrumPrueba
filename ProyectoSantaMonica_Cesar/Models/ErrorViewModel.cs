namespace ProyectoSantaMonica_Cesar.Models
{
    public class ReporteCmpFiltrosViewModel
    {
        public string? RequestId { get; set; }

        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }
}
