using Microsoft.EntityFrameworkCore;
using ProyectoSantaMonica_Cesar.Models;

namespace ProyectoSantaMonica_Cesar.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Usuario> Usuario { get; set; }
        public DbSet<Paciente> Paciente { get; set; }
        public DbSet<Medico> Medico { get; set; }
        public DbSet<Especialidad> Especialidad { get; set; }
        public DbSet<Horario> HorariosAtencion { get; set; }
        public DbSet<Cita> Cita { get; set; }
        public DbSet<ComprobanteDePago> ComprobanteDePago { get; set; }
        public DbSet<HistorialUsuario> HistorialUsuario { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ================================
            // USUARIO
            // ================================
            modelBuilder.Entity<Usuario>(entity =>
            {
                entity.ToTable("Usuario");

                // La columna Rol en SQL Server es VARCHAR(20).
                // Por eso EF Core debe guardar/leer el enum como texto.
                entity.Property(u => u.Rol)
                      .HasConversion<string>()
                      .HasMaxLength(20);
            });

            // ================================
            // MEDICO
            // ================================
            modelBuilder.Entity<Medico>(entity =>
            {
                entity.ToTable("Medico");

                entity.HasOne(m => m.especialidad)
                      .WithMany()
                      .HasForeignKey(m => m.Id_Especialidad)
                      .HasConstraintName("FK_Medico_Especialidad");
            });



            // ================================
            // ESPECIALIDAD
            // ================================
            modelBuilder.Entity<Especialidad>(entity =>
            {
                entity.ToTable("Especialidad");

                entity.HasKey(e => e.Id_Especialidad);

                entity.Property(e => e.Id_Especialidad)
                      .HasColumnName("Id_Especialidad");

                entity.Property(e => e.Nombre)
                      .HasColumnName("Nombre")
                      .HasMaxLength(100)
                      .IsRequired();
            });


            // ================================
            // COMPROBANTE
            // ================================
            modelBuilder.Entity<ComprobanteDePago>(entity =>
            {
                entity.ToTable("Comprobante_Pago");

                entity.Property(c => c.Metodo_Pago)
                      .HasConversion<string>();

                entity.Property(c => c.Estado)
                      .HasConversion<string>();
            });

            // ================================
            // HISTORIAL USUARIO
            // ================================
            modelBuilder.Entity<HistorialUsuario>(entity =>
            {
                entity.ToTable("Historial_Usuario");

                entity.HasIndex(h => new
                {
                    h.Id_Usuario,
                    h.Fecha
                });
            });

            // ================================
            // CITA
            // ================================
            modelBuilder.Entity<Cita>(entity =>
            {
                entity.Property(c => c.Estado)
                      .HasConversion<string>();
            });

            // ================================
            // HORARIO
            // ================================
            modelBuilder.Entity<Horario>(entity =>
            {
                entity.HasIndex(h => new
                {
                    h.Id_Medico,
                    h.Dia_Semana,
                    h.Horario_Entrada,
                    h.Horario_Salida
                }).IsUnique();

                entity.Property(h => h.Dia_Semana)
                      .HasConversion<string>();
            });
        }
    }
}