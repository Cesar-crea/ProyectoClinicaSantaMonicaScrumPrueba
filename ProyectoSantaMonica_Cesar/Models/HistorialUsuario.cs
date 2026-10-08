using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProyectoSantaMonica_Cesar.Models
{
    /// <summary>
    /// Auditoría funcional de las acciones realizadas por los usuarios autenticados.
    /// </summary>
    [Table("Historial_Usuario")]
    public class HistorialUsuario
    {
        [Key]
        [Column("Id_Historial")]
        public long Id_Historial { get; set; }

        [Required]
        [Column("Id_Usuario")]
        public long Id_Usuario { get; set; }

        [Required]
        [StringLength(100)]
        [Column("Accion")]
        public string Accion { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        [Column("Modulo")]
        public string Modulo { get; set; } = string.Empty;

        [StringLength(500)]
        [Column("Descripcion")]
        public string? Descripcion { get; set; }

        [Column("Fecha")]
        public DateTime Fecha { get; set; } = DateTime.Now;

        [StringLength(50)]
        [Column("Ip")]
        public string? Ip { get; set; }

        [ForeignKey(nameof(Id_Usuario))]
        public Usuario? Usuario { get; set; }
    }
}