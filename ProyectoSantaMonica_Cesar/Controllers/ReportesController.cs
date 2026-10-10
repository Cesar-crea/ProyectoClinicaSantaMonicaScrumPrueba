
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

        public ReportesController(
            IReporteService reportes,
            IMedicoService medicos,
            EspecialidadRepository especialidades,
            IHistorialUsuarioService historial)
        {
            _reportes = reportes;
            _medicos = medicos;
            _especialidades = especialidades;
            _historial = historial;
        }

        // =====================================================
        // REPORTE DE CITAS
        // ADMINISTRADOR y RECEPCIONISTA
        // =====================================================

        [HttpGet]
        public async Task<IActionResult> Index(
            ReporteFiltrosViewModel filtros)
        {
            ViewBag.Medicos = await _medicos.ListarTodosAsync();

            ViewBag.Especialidades = (await _especialidades.Listar())
                .Select(x => x.Nombre)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct()
                .OrderBy(x => x)
                .ToArray();

            ViewBag.Estados = new[]
            {
                "PENDIENTE",
                "PAGADO",
                "CANCELADO",
                "VENCIDO",
                "ATENDIDO"
            };

            var rows = await _reportes.ObtenerCitasAsync(filtros);

            ViewBag.Resultados = rows;

            return View(filtros);
        }

        [HttpGet]
        public async Task<IActionResult> CitasPdf(
            ReporteFiltrosViewModel filtros)
        {
            var rows = await _reportes.ObtenerCitasAsync(filtros);

            await Log(
                "GENERAR_PDF",
                "REPORTES",
                "Reporte de citas");

            var archivo = _reportes.GenerarCitasPdf(rows, filtros);

            return File(
                archivo,
                "application/pdf",
                $"reporte-citas-{DateTime.Now:yyyyMMddHHmmss}.pdf");
        }

        [HttpGet]
        public async Task<IActionResult> CitasExcel(
            ReporteFiltrosViewModel filtros)
        {
            var rows = await _reportes.ObtenerCitasAsync(filtros);

            await Log(
                "GENERAR_EXCEL",
                "REPORTES",
                "Reporte de citas");

            var archivo = _reportes.GenerarCitasExcel(rows, filtros);

            return File(
                archivo,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"reporte-citas-{DateTime.Now:yyyyMMddHHmmss}.xlsx");
        }

        // =====================================================
        // REPORTE CMP
        // SOLO ADMINISTRADOR
        // =====================================================

        [Authorize(Roles = "ADMINISTRADOR")]
        [HttpGet]
        public async Task<IActionResult> Cmp(
            ReporteCmpFiltrosViewModel filtros)
        {
            var todos = await _reportes.ObtenerCmpAsync();

            // Las opciones de especialidad salen de la lista completa.
            ViewBag.Especialidades = todos
                .Select(x => x.Especialidad)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct()
                .OrderBy(x => x)
                .ToList();

            // Se entregan a la vista todos los resultados filtrados.
            // La paginación corresponde a Cmp.cshtml.
            ViewBag.Resultados = FiltrarCmp(todos, filtros);

            return View(filtros);
        }

        [Authorize(Roles = "ADMINISTRADOR")]
        [HttpGet]
        public async Task<IActionResult> CmpPdf(
            ReporteCmpFiltrosViewModel filtros)
        {
            var todos = await _reportes.ObtenerCmpAsync();
            var rows = FiltrarCmp(todos, filtros);

            await Log(
                "GENERAR_PDF",
                "REPORTES_CMP",
                "Reporte CMP filtrado");

            var archivo = _reportes.GenerarCmpPdf(rows);

            return File(
                archivo,
                "application/pdf",
                $"reporte-cmp-{DateTime.Now:yyyyMMddHHmmss}.pdf");
        }

        [Authorize(Roles = "ADMINISTRADOR")]
        [HttpGet]
        public async Task<IActionResult> CmpExcel(
            ReporteCmpFiltrosViewModel filtros)
        {
            var todos = await _reportes.ObtenerCmpAsync();
            var rows = FiltrarCmp(todos, filtros);

            await Log(
                "GENERAR_EXCEL",
                "REPORTES_CMP",
                "Reporte CMP filtrado");

            var archivo = _reportes.GenerarCmpExcel(rows);

            return File(
                archivo,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"reporte-cmp-{DateTime.Now:yyyyMMddHHmmss}.xlsx");
        }

        // Método común para filtrar el reporte CMP.
        // Se utiliza tanto en pantalla como en las exportaciones.
        private static List<ReporteCmpRow> FiltrarCmp(
            IEnumerable<ReporteCmpRow> filas,
            ReporteCmpFiltrosViewModel filtros)
        {
            IEnumerable<ReporteCmpRow> query = filas;

            if (!string.IsNullOrWhiteSpace(filtros.Especialidad))
            {
                var especialidad = filtros.Especialidad.Trim();

                query = query.Where(x =>
                    string.Equals(
                        x.Especialidad,
                        especialidad,
                        StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(filtros.Buscar))
            {
                var buscar = filtros.Buscar.Trim();

                query = query.Where(x =>
                    (x.Medico ?? "").Contains(
                        buscar, StringComparison.OrdinalIgnoreCase)
                    || (x.Dni ?? "").Contains(
                        buscar, StringComparison.OrdinalIgnoreCase)
                    || (x.CMP ?? "").Contains(
                        buscar, StringComparison.OrdinalIgnoreCase)
                    || (x.Especialidad ?? "").Contains(
                        buscar, StringComparison.OrdinalIgnoreCase)
                    || (x.Telefono ?? "").Contains(
                        buscar, StringComparison.OrdinalIgnoreCase));
            }

            return query
                .OrderBy(x => x.Medico)
                .ToList();
        }

// =====================================================
// REPORTE DE HISTORIAL DE USUARIOS
// SOLO ADMINISTRADOR
// =====================================================

[Authorize(Roles = "ADMINISTRADOR")]
[HttpGet]
public async Task<IActionResult> Historial(
    ReporteHistorialFiltrosViewModel filtros)
        {
            // Normalizar los filtros de paginación.
            if (filtros.Pagina < 1)
                filtros.Pagina = 1;

            if (filtros.RegistrosPorPagina is not (10 or 25 or 50))
                filtros.RegistrosPorPagina = 10;

            // Obtener los registros según usuario y fechas.
            var todos = await _historial.ListarAsync(
                filtros.IdUsuario,
                filtros.Desde,
                filtros.Hasta);

            // Aplicar búsqueda de texto.
            IEnumerable<ProyectoSantaMonica_Cesar.Models.HistorialUsuario> consulta = todos;

            if (!string.IsNullOrWhiteSpace(filtros.Buscar))
            {
                var buscar = filtros.Buscar.Trim();

                consulta = consulta.Where(x =>
                    (x.Usuario?.Username ?? "")
                        .Contains(buscar, StringComparison.OrdinalIgnoreCase)
                    || (x.Usuario == null ? "" : x.Usuario.Rol.ToString())
    .Contains(buscar, StringComparison.OrdinalIgnoreCase)
                    || (x.Modulo ?? "")
                        .Contains(buscar, StringComparison.OrdinalIgnoreCase)
                    || (x.Accion ?? "")
                        .Contains(buscar, StringComparison.OrdinalIgnoreCase)
                    || (x.Descripcion ?? "")
                        .Contains(buscar, StringComparison.OrdinalIgnoreCase)
                    || (x.Ip ?? "")
                        .Contains(buscar, StringComparison.OrdinalIgnoreCase)
                    || x.Id_Usuario.ToString().Contains(buscar));
            }

            var resultados = consulta
                .OrderByDescending(x => x.Fecha)
                .ToList();

            // Calcular la paginación.
            var totalRegistros = resultados.Count;
            var totalPaginas = Math.Max(
                1,
                (int)Math.Ceiling(
                    totalRegistros / (double)filtros.RegistrosPorPagina));

            if (filtros.Pagina > totalPaginas)
                filtros.Pagina = totalPaginas;

            var paginaActual = resultados
                .Skip((filtros.Pagina - 1) * filtros.RegistrosPorPagina)
                .Take(filtros.RegistrosPorPagina)
                .ToList();

            // Datos que utilizará la vista.
            ViewBag.Resultados = paginaActual;
            ViewBag.TotalRegistros = totalRegistros;
            ViewBag.TotalPaginas = totalPaginas;

            return View(filtros);
        }


        [Authorize(Roles = "ADMINISTRADOR")]
        [HttpGet]
        public async Task<IActionResult> HistorialExcel(
            ReporteHistorialFiltrosViewModel filtros)
        {
            var rows = await ObtenerHistorialParaExportarAsync(filtros);

            await Log(
                "GENERAR_EXCEL",
                "HISTORIAL_USUARIOS",
                "Exportación del historial de usuarios a Excel");

            var archivo = _reportes.GenerarHistorialExcel(rows);

            return File(
                archivo,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"historial-usuarios-{DateTime.Now:yyyyMMddHHmmss}.xlsx");
        }

        [Authorize(Roles = "ADMINISTRADOR")]
        [HttpGet]
        public async Task<IActionResult> HistorialPdf(
            ReporteHistorialFiltrosViewModel filtros)
        {
            var rows = await ObtenerHistorialParaExportarAsync(filtros);

            await Log(
                "GENERAR_PDF",
                "HISTORIAL_USUARIOS",
                "Exportación del historial de usuarios a PDF");

            var archivo = _reportes.GenerarHistorialPdf(rows);

            return File(
                archivo,
                "application/pdf",
                $"historial-usuarios-{DateTime.Now:yyyyMMddHHmmss}.pdf");
        }

        private async Task<List<ProyectoSantaMonica_Cesar.Models.HistorialUsuario>>
            ObtenerHistorialParaExportarAsync(
                ReporteHistorialFiltrosViewModel filtros)
        {
            var todos = await _historial.ListarAsync(
                filtros.IdUsuario,
                filtros.Desde,
                filtros.Hasta);

            IEnumerable<ProyectoSantaMonica_Cesar.Models.HistorialUsuario> consulta = todos;

            if (!string.IsNullOrWhiteSpace(filtros.Buscar))
            {
                var buscar = filtros.Buscar.Trim();

                consulta = consulta.Where(x =>
                    (x.Usuario?.Username ?? "")
                        .Contains(buscar, StringComparison.OrdinalIgnoreCase)
                    || (x.Usuario == null ? "" : x.Usuario.Rol.ToString())
                        .Contains(buscar, StringComparison.OrdinalIgnoreCase)
                    || (x.Modulo ?? "")
                        .Contains(buscar, StringComparison.OrdinalIgnoreCase)
                    || (x.Accion ?? "")
                        .Contains(buscar, StringComparison.OrdinalIgnoreCase)
                    || (x.Descripcion ?? "")
                        .Contains(buscar, StringComparison.OrdinalIgnoreCase)
                    || (x.Ip ?? "")
                        .Contains(buscar, StringComparison.OrdinalIgnoreCase)
                    || x.Id_Usuario.ToString().Contains(buscar));
            }

            return consulta
                .OrderByDescending(x => x.Fecha)
                .ToList();
        }


     
// =====================================================
// HISTORIAL DE COMPROBANTES DE PAGO
// ADMINISTRADOR Y RECEPCIONISTA
// =====================================================

[HttpGet]
public async Task<IActionResult> HistorialComprobantes(
    ReporteComprobantePagoFiltrosViewModel filtros)
        {
            if (filtros.Pagina < 1)
                filtros.Pagina = 1;

            if (filtros.RegistrosPorPagina is not (10 or 25 or 50))
                filtros.RegistrosPorPagina = 10;

            var todos = await _reportes.ObtenerComprobantesAsync(filtros);

            var totalRegistros = todos.Count;

            var totalPaginas = Math.Max(
                1,
                (int)Math.Ceiling(
                    totalRegistros / (double)filtros.RegistrosPorPagina));

            if (filtros.Pagina > totalPaginas)
                filtros.Pagina = totalPaginas;

            var resultados = todos
                .Skip((filtros.Pagina - 1) * filtros.RegistrosPorPagina)
                .Take(filtros.RegistrosPorPagina)
                .ToList();

            ViewBag.Resultados = resultados;
            ViewBag.TotalRegistros = totalRegistros;
            ViewBag.TotalPaginas = totalPaginas;

            return View(filtros);
        }

        [HttpGet]
        public async Task<IActionResult> HistorialComprobantesExcel(
            ReporteComprobantePagoFiltrosViewModel filtros)
        {
            var rows = await _reportes.ObtenerComprobantesAsync(filtros);

            await Log(
                "GENERAR_EXCEL",
                "HISTORIAL_COMPROBANTES",
                "Exportación del historial de comprobantes a Excel");

            var archivo = _reportes.GenerarComprobantesExcel(rows);

            return File(
                archivo,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"historial-comprobantes-{DateTime.Now:yyyyMMddHHmmss}.xlsx");
        }

        [HttpGet]
        public async Task<IActionResult> HistorialComprobantesPdf(
            ReporteComprobantePagoFiltrosViewModel filtros)
        {
            var rows = await _reportes.ObtenerComprobantesAsync(filtros);

            await Log(
                "GENERAR_PDF",
                "HISTORIAL_COMPROBANTES",
                "Exportación del historial de comprobantes a PDF");

            var archivo = _reportes.GenerarComprobantesPdf(rows);

            return File(
                archivo,
                "application/pdf",
                $"historial-comprobantes-{DateTime.Now:yyyyMMddHHmmss}.pdf");
        }



        // =====================================================
        // REGISTRO DE AUDITORÍA
        // =====================================================

        private Task Log(
            string accion,
            string modulo,
            string descripcion)
        {
            var id = User.FindFirstValue("Id_Usuario");

            if (long.TryParse(id, out var uid))
            {
                return _historial.RegistrarAsync(
                    uid,
                    accion,
                    modulo,
                    descripcion,
                    HttpContext.Connection.RemoteIpAddress?.ToString());
            }

            return Task.CompletedTask;
        }
    }
}
