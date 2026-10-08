using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProyectoSantaMonica_Cesar.Models
{
    [Table("Especialidad")]
    public class Especialidad
    {
        [Key]
        [Column("Id_Especialidad")]
        public int Id_Especialidad { get; set; }

        [Required(ErrorMessage = "El nombre de la especialidad es obligatorio")]
        [StringLength(100, ErrorMessage = "Máximo 100 caracteres")]
        [Column("Nombre")]
        public string Nombre { get; set; } = string.Empty;
    }
}