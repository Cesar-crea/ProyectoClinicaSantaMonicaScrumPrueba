using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProyectoSantaMonica_Cesar.Models.ViewModels
{
    public class ReporteHistorialFiltrosViewModel
    {
        public long? IdUsuario { get; set; }

        public DateTime? Desde { get; set; }

        public DateTime? Hasta { get; set; }

        public string? Buscar { get; set; }

        public int Pagina { get; set; } = 1;

        public int RegistrosPorPagina { get; set; } = 10;
    }
}