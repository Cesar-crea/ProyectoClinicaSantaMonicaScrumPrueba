using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProyectoSantaMonica_Cesar.Models
{
    [Table("Medico")]
    public class Medico
    {
        [Key]
        [Column("Id_Medico")]
        public int Id_Medico { get; set; }

        [Required(ErrorMessage = "Los nombres son obligatorios")]
        [StringLength(100)]
        [Column("Nombres")]
        public string Nombres { get; set; } = string.Empty;

        [Required(ErrorMessage = "Los apellidos son obligatorios")]
        [StringLength(100)]
        [Column("Apellidos")]
        public string Apellidos { get; set; } = string.Empty;

        [Required(ErrorMessage = "El DNI es obligatorio")]
        [StringLength(8, MinimumLength = 8)]
        [RegularExpression(@"^\d{8}$")]
        [Column("Dni")]
        public string Dni { get; set; } = string.Empty;

        [Required(ErrorMessage = "El número de colegiatura es obligatorio")]
        [StringLength(20)]
        [Column("Nro_Colegiatura")]
        public string Nro_Colegiatura { get; set; } = string.Empty;

        [Required(ErrorMessage = "El número Telefono es obligatorio")]
        [StringLength(9, MinimumLength = 7,
            ErrorMessage = "El teléfono debe tener entre 7 y 9 dígitos")]
        [Column("Telefono")]
        public string Telefono { get; set; } = string.Empty;

        [Required(ErrorMessage = "Debe seleccionar una especialidad")]
        [Column("Id_Especialidad")]
        public int Id_Especialidad { get; set; }

        // Solo para mostrar información calculada.
        // NO corresponde a una columna de la tabla Medico.
        [NotMapped]
        public string? Especialidad { get; set; }

        // Relación Medico -> Especialidad
        [ForeignKey(nameof(Id_Especialidad))]
        public Especialidad? especialidad { get; set; }
    }
}