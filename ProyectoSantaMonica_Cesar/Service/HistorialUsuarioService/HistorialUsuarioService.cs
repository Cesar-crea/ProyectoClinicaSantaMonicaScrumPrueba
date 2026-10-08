using Microsoft.EntityFrameworkCore;
using ProyectoSantaMonica_Cesar.Data;
using ProyectoSantaMonica_Cesar.Models;

namespace ProyectoSantaMonica_Cesar.Service.HistorialUsuarioService
{
    public class HistorialUsuarioService : IHistorialUsuarioService
    {
        private readonly ApplicationDbContext _context;
        public HistorialUsuarioService(ApplicationDbContext context) => _context = context;

        public async Task RegistrarAsync(
         long idUsuario,
         string accion,
         string modulo,
         string? descripcion = null,
         string? ip = null)
        {
            if (idUsuario <= 0)
                return;

            try
            {
                _context.HistorialUsuario.Add(new HistorialUsuario
                {
                    Id_Usuario = idUsuario,
                    Accion = accion,
                    Modulo = modulo,
                    Descripcion = descripcion,
                    Ip = ip,
                    Fecha = DateTime.Now
                });

                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                Console.WriteLine("========================================");
                Console.WriteLine("ERROR AL GUARDAR HISTORIAL");
                Console.WriteLine("========================================");
                Console.WriteLine(ex.Message);

                if (ex.InnerException != null)
                {
                    Console.WriteLine("INNER EXCEPTION:");
                    Console.WriteLine(ex.InnerException.Message);
                }

                throw;
            }
        }

        public Task<List<HistorialUsuario>> ListarAsync(long? idUsuario = null, DateTime? desde = null, DateTime? hasta = null)
        {
            var q = _context.HistorialUsuario.Include(x => x.Usuario).AsNoTracking().AsQueryable();
            if (idUsuario.HasValue) q = q.Where(x => x.Id_Usuario == idUsuario.Value);
            if (desde.HasValue) q = q.Where(x => x.Fecha >= desde.Value.Date);
            if (hasta.HasValue) q = q.Where(x => x.Fecha < hasta.Value.Date.AddDays(1));
            return q.OrderByDescending(x => x.Fecha).Take(500).ToListAsync();
        }
    }
}
