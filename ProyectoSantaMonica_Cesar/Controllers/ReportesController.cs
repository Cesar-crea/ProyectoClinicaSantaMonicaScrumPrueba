using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProyectoSantaMonica_Cesar.Models.ViewModels;
using ProyectoSantaMonica_Cesar.Service.HistorialUsuarioService;
using ProyectoSantaMonica_Cesar.Service.MedicoService;
using ProyectoSantaMonica_Cesar.Repository;
using ProyectoSantaMonica_Cesar.Service.ReporteService;
using System.Security.Claims;

namespace ProyectoSantaMonica_Cesar.Controllers
{
    [Authorize(Roles = "ADMINISTRADOR,RECEPCIONISTA")]
    public class ReportesController : Controller
    {
        private readonly IReporteService _reportes;
        private readonly IMedicoService _medicos;
        private readonly EspecialidadRepository _especialidades;
        private readonly IHistorialUsuarioService _historial;
        public ReportesController(IReporteService reportes, IMedicoService medicos, EspecialidadRepository especialidades, IHistorialUsuarioService historial)
        { _reportes=reportes; _medicos=medicos; _especialidades=especialidades; _historial=historial; }


        [HttpGet]
        public async Task<IActionResult> Index(
    ReporteFiltrosViewModel filtros)
        {
            ViewBag.Medicos =
                await _medicos.ListarTodosAsync();

            ViewBag.Especialidades =
                (await _especialidades.Listar())
                .Select(x => x.Nombre)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct()
                .OrderBy(x => x)
                .ToArray();

            ViewBag.Estados =
                new[]
                {
            "PENDIENTE",
            "PAGADO",
            "CANCELADO",
            "VENCIDO",
            "ATENDIDO"
                };

            var rows =
                await _reportes.ObtenerCitasAsync(filtros);

            ViewBag.Resultados = rows;

            return View(filtros);
        }

        [HttpGet]
        public async Task<IActionResult> CitasPdf(ReporteFiltrosViewModel filtros)
        { var rows=await _reportes.ObtenerCitasAsync(filtros); await Log("GENERAR_PDF","REPORTES","Reporte de citas"); return File(_reportes.GenerarCitasPdf(rows,filtros),"application/pdf",$"reporte-citas-{DateTime.Now:yyyyMMddHHmmss}.pdf"); }

        [HttpGet]
        public async Task<IActionResult> CitasExcel(ReporteFiltrosViewModel filtros)
        { var rows=await _reportes.ObtenerCitasAsync(filtros); await Log("GENERAR_EXCEL","REPORTES","Reporte de citas"); return File(_reportes.GenerarCitasExcel(rows,filtros),"application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",$"reporte-citas-{DateTime.Now:yyyyMMddHHmmss}.xlsx"); }

        [Authorize(Roles = "ADMINISTRADOR")]
        public async Task<IActionResult> CmpPdf()
        { var rows=await _reportes.ObtenerCmpAsync(); await Log("GENERAR_PDF","REPORTES_CMP","Reporte CMP"); return File(_reportes.GenerarCmpPdf(rows),"application/pdf",$"reporte-cmp-{DateTime.Now:yyyyMMddHHmmss}.pdf"); }

        [Authorize(Roles = "ADMINISTRADOR")]
        public async Task<IActionResult> CmpExcel()
        { var rows=await _reportes.ObtenerCmpAsync(); await Log("GENERAR_EXCEL","REPORTES_CMP","Reporte CMP"); return File(_reportes.GenerarCmpExcel(rows),"application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",$"reporte-cmp-{DateTime.Now:yyyyMMddHHmmss}.xlsx"); }

        [Authorize(Roles = "ADMINISTRADOR")]
        public async Task<IActionResult> Historial(long? idUsuario, DateTime? desde, DateTime? hasta)
        { var rows=await _historial.ListarAsync(idUsuario,desde,hasta); return View(rows); }

        private Task Log(string accion,string modulo,string descripcion) { var id=User.FindFirstValue("Id_Usuario"); return long.TryParse(id,out var uid) ? _historial.RegistrarAsync(uid,accion,modulo,descripcion,HttpContext.Connection.RemoteIpAddress?.ToString()) : Task.CompletedTask; }
    }
}
