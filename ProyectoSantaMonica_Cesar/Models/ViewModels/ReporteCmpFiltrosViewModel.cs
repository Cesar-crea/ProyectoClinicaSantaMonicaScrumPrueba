namespace ProyectoSantaMonica_Cesar.Models.ViewModels
{
    public class ReporteCmpFiltrosViewModel
    {
        public string? Buscar { get; set; }

        public string? Especialidad { get; set; }

        public int Pagina { get; set; } = 1;

        public int RegistrosPorPagina { get; set; } = 10;
    }
}