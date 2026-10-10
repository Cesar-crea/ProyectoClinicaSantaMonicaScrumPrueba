using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using ProyectoSantaMonica_Cesar.Models.enums;

using System;

namespace ProyectoSantaMonica_Cesar.Models.ViewModels
{
    public class ReporteComprobantePagoFiltrosViewModel
    {
        public DateTime? Desde { get; set; }
        public DateTime? Hasta { get; set; }
        public string? Estado { get; set; }
        public string? Metodo { get; set; }
        public string? Buscar { get; set; }

        public int Pagina { get; set; } = 1;
        public int RegistrosPorPagina { get; set; } = 10;
    }

    public class ReporteComprobantePagoRow
    {
        public int Id_Comprobante { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Apellidos { get; set; } = string.Empty;
        public string Dni { get; set; } = string.Empty;
        public DateTime Fecha { get; set; }
        public decimal Monto { get; set; }
        public string Metodo { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
    }
}