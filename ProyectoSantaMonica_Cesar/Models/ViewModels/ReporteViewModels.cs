namespace ProyectoSantaMonica_Cesar.Models.ViewModels
{
    public class ReporteFiltrosViewModel
    {
        public DateTime Desde { get; set; } = DateTime.Today.AddDays(-30);
        public DateTime Hasta { get; set; } = DateTime.Today;
        public int? Id_Medico { get; set; }
        public string? Estado { get; set; }
        public string? Especialidad { get; set; }
    }

    public class ReporteCitaRow
    {
        public int Id_Cita { get; set; }
        public string Paciente { get; set; } = string.Empty;
        public string Dni { get; set; } = string.Empty;
        public string Medico { get; set; } = string.Empty;
        public string Especialidad { get; set; } = string.Empty;
        public DateTime Fecha { get; set; }
        public TimeSpan Hora { get; set; }
        public string Estado { get; set; } = string.Empty;
        public string Motivo { get; set; } = string.Empty;
    }

    public class ReporteCmpRow
    {
        public int Id_Medico { get; set; }
        public string Medico { get; set; } = string.Empty;
        public string Dni { get; set; } = string.Empty;
        public string CMP { get; set; } = string.Empty;
        public string Especialidad { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;
    }
}
