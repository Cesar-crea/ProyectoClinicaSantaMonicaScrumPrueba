using ProyectoSantaMonica_Cesar.Models;

namespace ProyectoSantaMonica_Cesar.Service.HistorialUsuarioService
{
    public interface IHistorialUsuarioService
    {
        Task RegistrarAsync(long idUsuario, string accion, string modulo, string? descripcion = null, string? ip = null);
        Task<List<HistorialUsuario>> ListarAsync(long? idUsuario = null, DateTime? desde = null, DateTime? hasta = null);
    }
}
