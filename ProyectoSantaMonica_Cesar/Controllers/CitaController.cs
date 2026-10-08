using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ProyectoSantaMonica_Cesar.Service.HistorialUsuarioService;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using ProyectoSantaMonica_Cesar.Models;
using ProyectoSantaMonica_Cesar.Models.enums;
using ProyectoSantaMonica_Cesar.Models.ViewModels;
using ProyectoSantaMonica_Cesar.Service.CitaService;
using ProyectoSantaMonica_Cesar.Service.MedicoService;
using ProyectoSantaMonica_Cesar.Service.PacienteService;
using ProyectoSantaMonica_Cesar.Data;

namespace ProyectoSantaMonica_Cesar.Controllers
{
    [Authorize(Roles = "ADMINISTRADOR,RECEPCIONISTA")]
    public class CitaController : Controller
    {
        private readonly ICitaService _service;
        private readonly IPacienteService _pacienteservice;
        private readonly IMedicoService _medicoservice;
        private readonly ApplicationDbContext _context;
        private readonly IHistorialUsuarioService _historial;

        public CitaController(
            ICitaService service,
            IPacienteService pacienteService,
            IMedicoService medicoService,
            ApplicationDbContext context,
            IHistorialUsuarioService historial)
        {
            _service = service;
            _pacienteservice = pacienteService;
            _medicoservice = medicoService;
            _context = context;
            _historial = historial;
        }


        // =====================================================
        // LISTAR CITAS
        //
        // REGLA FUNCIONAL:
        //
        // 1. Sin búsqueda:
        //    - Por defecto muestra solamente PENDIENTES.
        //
        // 2. Con búsqueda por paciente:
        //    - Muestra TODAS las citas que coincidan.
        //    - No importa el estado.
        //    - PENDIENTE aparece primero.
        //
        // 3. Ver más:
        //    - Muestra TODAS las citas.
        //    - PENDIENTE aparece primero.
        // =====================================================

        [HttpGet]
        public async Task<IActionResult> Listar(
            string? texto,
            bool verTodas = false)
        {
            try
            {
                List<Cita> lista;

                // -------------------------------------------------
                // Actualizar automáticamente los estados
                // -------------------------------------------------

                await _service.ActualizarEstadosAutomaticosAsync();

                texto = texto?.Trim();

                // =================================================
                // 1. HAY BÚSQUEDA
                //
                // La búsqueda SIEMPRE consulta todas las citas.
                // No importa si están:
                // PENDIENTE
                // PAGADO
                // ATENDIDO
                // CANCELADO
                // VENCIDO
                // =================================================

                if (!string.IsNullOrWhiteSpace(texto))
                {
                    var todas = await _service.ListarAsync();

                    lista = todas
                        .Where(c =>
                            c.Paciente != null &&
                            (
                                c.Paciente.Nombres.Contains(
                                    texto,
                                    StringComparison.OrdinalIgnoreCase)

                                ||

                                c.Paciente.Apellidos.Contains(
                                    texto,
                                    StringComparison.OrdinalIgnoreCase)

                                ||

                                $"{c.Paciente.Nombres} {c.Paciente.Apellidos}"
                                    .Contains(
                                        texto,
                                        StringComparison.OrdinalIgnoreCase)
                            )
                        )
                        // PENDIENTE SIEMPRE PRIMERO
                        .OrderBy(c =>
                            c.Estado == EstadoCita.PENDIENTE ? 0 : 1)

                        // Luego fecha
                        .ThenBy(c => c.Fecha)

                        // Luego hora
                        .ThenBy(c => c.Hora)

                        .ToList();
                }

                // =================================================
                // 2. SIN BÚSQUEDA
                //    Y SIN "VER MÁS"
                //
                // Mostrar solamente PENDIENTES
                // =================================================

                else if (!verTodas)
                {
                    lista = await _service.ListarPorEstadoAsync(
                        EstadoCita.PENDIENTE);

                    lista = lista
                        .OrderBy(c => c.Fecha)
                        .ThenBy(c => c.Hora)
                        .ToList();
                }

                // =================================================
                // 3. SIN BÚSQUEDA
                //    Y CON "VER MÁS"
                //
                // Mostrar todas las citas
                // =================================================

                else
                {
                    lista = await _service.ListarAsync();

                    lista = lista
                        // PENDIENTE SIEMPRE PRIMERO
                        .OrderBy(c =>
                            c.Estado == EstadoCita.PENDIENTE ? 0 : 1)

                        // Luego fecha
                        .ThenBy(c => c.Fecha)

                        // Luego hora
                        .ThenBy(c => c.Hora)

                        .ToList();
                }

                // -------------------------------------------------
                // Datos para la vista
                // -------------------------------------------------

                ViewBag.Texto = texto;
                ViewBag.VerTodas = verTodas;

                return View(lista);
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;

                ViewBag.Texto = texto;
                ViewBag.VerTodas = verTodas;

                return View(new List<Cita>());
            }
        }

// ==========================================================
// REGISTRAR - GET
// ==========================================================

[HttpGet]
public async Task<IActionResult> Registrar()
        {
            ViewBag.Pacientes =
                _pacienteservice.ListarTodosPaciente();

            ViewBag.Medicos =
                await _medicoservice.ListarTodosAsync();

            var cita = new Cita
            {
                // La fecha inicia en HOY
                Fecha = DateTime.Today,

                // Nueva cita inicia como PENDIENTE
                Estado = EstadoCita.PENDIENTE
            };

            return View(cita);
        }


         
        // ==========================================================
        // REGISTRAR NUEVA CITA - POST
        // ==========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Registrar(Cita c)
        {
            try
            {
                // ==================================================
                // FECHA
                // ==================================================

                c.Fecha = c.Fecha.Date;

                // Nunca permitir fechas anteriores a hoy
                if (c.Fecha < DateTime.Today)
                {
                    TempData["Error"] =
                        "No se pueden registrar citas para fechas anteriores a hoy.";

                    ViewBag.Pacientes =
                        _pacienteservice.ListarTodosPaciente();

                    ViewBag.Medicos =
                        await _medicoservice.ListarTodosAsync();

                    return View(c);
                }


                // ==================================================
                // USUARIO LOGUEADO
                // ==================================================

                var usuarioId =
                    User.FindFirst("Id_Usuario")?.Value;

                if (string.IsNullOrWhiteSpace(usuarioId))
                {
                    TempData["Error"] =
                        "No se pudo identificar al usuario que registra la cita.";

                    ViewBag.Pacientes =
                        _pacienteservice.ListarTodosPaciente();

                    ViewBag.Medicos =
                        await _medicoservice.ListarTodosAsync();

                    return View(c);
                }

                c.Id_Usuario =
                    Convert.ToInt64(usuarioId);


                // ==================================================
                // VALIDAR MÉDICO
                // ==================================================

                if (c.Id_Medico <= 0)
                {
                    TempData["Error"] =
                        "Debe seleccionar un médico.";

                    ViewBag.Pacientes =
                        _pacienteservice.ListarTodosPaciente();

                    ViewBag.Medicos =
                        await _medicoservice.ListarTodosAsync();

                    return View(c);
                }


                // ==================================================
                // VALIDAR PACIENTE
                // ==================================================

                if (c.Id_Paciente <= 0)
                {
                    TempData["Error"] =
                        "Debe seleccionar un paciente.";

                    ViewBag.Pacientes =
                        _pacienteservice.ListarTodosPaciente();

                    ViewBag.Medicos =
                        await _medicoservice.ListarTodosAsync();

                    return View(c);
                }


                // ==================================================
                // VALIDAR HORA
                // ==================================================

                if (c.Hora == TimeSpan.Zero)
                {
                    TempData["Error"] =
                        "Debe seleccionar una hora.";

                    ViewBag.Pacientes =
                        _pacienteservice.ListarTodosPaciente();

                    ViewBag.Medicos =
                        await _medicoservice.ListarTodosAsync();

                    return View(c);
                }


                // ==================================================
                // VALIDAR HORARIO REAL DEL MÉDICO
                // ==================================================

                bool atiende =
                    await _service.MedicoAtiendeEnHorarioAsync(
                        c.Id_Medico,
                        c.Fecha,
                        c.Hora);

                if (!atiende)
                {
                    TempData["Error"] =
                        "El médico no atiende en el horario seleccionado o la hora ya está ocupada.";

                    ViewBag.Pacientes =
                        _pacienteservice.ListarTodosPaciente();

                    ViewBag.Medicos =
                        await _medicoservice.ListarTodosAsync();

                    return View(c);
                }


                // ==================================================
                // REGISTRAR CITA
                // ==================================================

                var msg =
                    await _service.RegistrarAsync(c);

                if (msg == "OK")
                {
                    await LogAsync(
                        "REGISTRAR",
                        "CITAS",
                        $"Cita #{c.Id_Cita} registrada"
                    );

                    TempData["Success"] =
                        "Cita registrada correctamente.";

                    return RedirectToAction(nameof(Listar));
                }


                TempData["Error"] = msg;

                ViewBag.Pacientes =
                    _pacienteservice.ListarTodosPaciente();

                ViewBag.Medicos =
                    await _medicoservice.ListarTodosAsync();

                return View(c);
            }
            catch (Exception ex)
            {
                TempData["Error"] =
                    ex.Message;

                ViewBag.Pacientes =
                    _pacienteservice.ListarTodosPaciente();

                ViewBag.Medicos =
                    await _medicoservice.ListarTodosAsync();

                return View(c);
            }
        }


        // ==========================================================
        // HORAS DISPONIBLES PARA MÉDICO + FECHA
        // ==========================================================

        [HttpGet]
        public async Task<IActionResult> HorasDisponibles(
            int idMedico,
            DateTime fecha)
        {
            try
            {
                // ------------------------------------------
                // VALIDACIONES
                // ------------------------------------------

                if (idMedico <= 0)
                {
                    return Json(new
                    {
                        ok = false,
                        mensaje = "Debe seleccionar un médico."
                    });
                }


                fecha = fecha.Date;

                if (fecha < DateTime.Today)
                {
                    return Json(new
                    {
                        ok = false,
                        mensaje =
                            "No se pueden consultar fechas anteriores a hoy."
                    });
                }


                // ------------------------------------------
                // DÍA DE LA SEMANA
                // ------------------------------------------

                var diaSemana =
                    DIaSemanaHelper.FromDateTime(fecha);


                // ------------------------------------------
                // HORARIOS DEL MÉDICO
                // ------------------------------------------

                var horarios =
                    await _medicoservice
                        .ObtenerHorariosPorMedicoAsync(idMedico);

                var horariosDelDia =
                    horarios
                        .Where(h =>
                            h.Dia_Semana.HasValue &&
                            h.Dia_Semana.Value == diaSemana &&
                            h.Horario_Entrada.HasValue &&
                            h.Horario_Salida.HasValue)
                        .OrderBy(h => h.Horario_Entrada)
                        .ToList();

                if (!horariosDelDia.Any())
                {
                    return Json(new
                    {
                        ok = true,

                        mensaje =
                            "El médico no tiene horario de atención para este día.",

                        horas = Array.Empty<string>()
                    });
                }


                // ------------------------------------------
                // CITAS YA OCUPADAS
                // ------------------------------------------

                var ocupadas =
                    await _service.HorariosOcupadosAsync(
                        idMedico,
                        fecha);

                var horasOcupadas =
                    ocupadas
                        .Select(x => x.Hora)
                        .ToHashSet();


                // ------------------------------------------
                // GENERAR HORAS DISPONIBLES
                //
                // La interfaz actual trabaja con bloques
                // de una hora.
                // ------------------------------------------

                var horasDisponibles =
                    new List<string>();

                foreach (var horario in horariosDelDia)
                {
                    var inicio =
                        horario.Horario_Entrada!.Value;

                    var salida =
                        horario.Horario_Salida!.Value;

                    for (
                        var hora = inicio;
                        hora < salida;
                        hora = hora.Add(TimeSpan.FromHours(1)))
                    {
                        if (!horasOcupadas.Contains(hora))
                        {
                            horasDisponibles.Add(
                                hora.ToString(@"hh\:mm"));
                        }
                    }
                }


                horasDisponibles =
                    horasDisponibles
                        .Distinct()
                        .OrderBy(x => x)
                        .ToList();


                // ------------------------------------------
                // RESPUESTA
                // ------------------------------------------

                return Json(new
                {
                    ok = true,

                    mensaje = horasDisponibles.Any()
                        ? ""
                        : "No hay horas disponibles para esta fecha.",

                    horas = horasDisponibles
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    ok = false,
                    mensaje = ex.Message
                });
            }
        }


        // ==========================================================
        // ACTUALIZAR
        // ==========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Actualizar(Cita c)
        {
            try
            {
                var msg =
                    await _service.ActualizarAsync(c);

                TempData[
                    msg == "OK"
                        ? "Success"
                        : "Error"
                ] =
                    msg == "OK"
                        ? "Cita actualizada correctamente"
                        : msg;
            }
            catch (Exception ex)
            {
                TempData["Error"] =
                    ex.Message;
            }


            await LogAsync(
                "ACTUALIZAR",
                "CITAS",
                $"Cita #{c.Id_Cita} actualizada"
            );

            return RedirectToAction(nameof(Listar));
        }


        // =====================================================
        // DESACTIVAR CITA
        //
        // NO SE ELIMINA DE LA BASE DE DATOS.
        //
        // Se cambia el estado a CANCELADO.
        // =====================================================

        [HttpGet]
        public async Task<IActionResult> Desactivar(int id)
        {
            try
            {
                var cita =
                    await _service.ObtenerPorIdAsync(id);

                if (cita == null)
                {
                    TempData["Error"] =
                        "Cita no encontrada";

                    return RedirectToAction(nameof(Listar));
                }


                // ---------------------------------------------
                // Solo se pueden desactivar citas pendientes
                // ---------------------------------------------

                if (cita.Estado != EstadoCita.PENDIENTE)
                {
                    TempData["Error"] =
                        "Solo se pueden desactivar citas pendientes.";

                    return RedirectToAction(nameof(Listar));
                }


                // ---------------------------------------------
                // CAMBIO DE ESTADO
                // ---------------------------------------------

                var msg =
                    await _service.ActualizarEstadoAsync(
                        id,
                        EstadoCita.CANCELADO
                    );

                TempData[
                    msg == "OK"
                        ? "Success"
                        : "Error"
                ] =
                    msg == "OK"
                        ? "Cita desactivada correctamente"
                        : msg;
            }
            catch (Exception ex)
            {
                TempData["Error"] =
                    ex.Message;
            }


            // ---------------------------------------------
            // AUDITORÍA
            // ---------------------------------------------

            await LogAsync(
                "DESACTIVAR",
                "CITAS",
                $"Cita #{id} desactivada"
            );

            return RedirectToAction(nameof(Listar));
        }


        // ==========================================================
        // DETALLE
        // ==========================================================

        public async Task<IActionResult> Detalle(int id)
        {
            try
            {
                var cita =
                    await _service.ObtenerPorIdAsync(id);

                if (cita == null)
                {
                    TempData["Error"] =
                        "Cita no encontrada";

                    return RedirectToAction(nameof(Listar));
                }

                return View(cita);
            }
            catch (Exception ex)
            {
                TempData["Error"] =
                    ex.Message;

                return RedirectToAction(nameof(Listar));
            }
        }


        // ==========================================================
        // CANCELAR
        // ==========================================================

        public async Task<IActionResult> Cancelar(int id)
        {
            try
            {
                var cita =
                    await _service.ObtenerPorIdAsync(id);

                if (cita == null)
                {
                    TempData["Error"] =
                        "Cita no encontrada";

                    return RedirectToAction(nameof(Listar));
                }


                if (cita.Estado != EstadoCita.PENDIENTE)
                {
                    TempData["Error"] =
                        "Solo se pueden cancelar citas pendientes";

                    return RedirectToAction(nameof(Listar));
                }


                var msg =
                    await _service.ActualizarEstadoAsync(
                        id,
                        EstadoCita.CANCELADO
                    );

                TempData[
                    msg == "OK"
                        ? "Success"
                        : "Error"
                ] =
                    msg == "OK"
                        ? "Cita cancelada correctamente"
                        : msg;
            }
            catch (Exception ex)
            {
                TempData["Error"] =
                    ex.Message;
            }


            await LogAsync(
                "CANCELAR",
                "CITAS",
                $"Cita #{id} cancelada"
            );

            return RedirectToAction(nameof(Listar));
        }


        // ==========================================================
        // FILTRAR POR ESTADO
        // ==========================================================

        public async Task<IActionResult> PorEstado(
            EstadoCita estado)
        {
            var lista =
                await _service.ListarPorEstadoAsync(estado);

            return View("Listar", lista);
        }


        // ==========================================================
        // PENDIENTES POR PACIENTE
        //
        // Este método se conserva porque puede ser utilizado
        // por otras partes del sistema.
        //
        // IMPORTANTE:
        // La búsqueda principal de Listar() NO utiliza este método,
        // porque ahora debe consultar todos los estados.
        // ==========================================================

        public async Task<IActionResult> Pendientes(
            string dato)
        {
            var lista =
                await _service.PendientesPorPacienteAsync(dato);

            return View("Listar", lista);
        }


        // ==========================================================
        // ACTUALIZAR ESTADOS AUTOMÁTICOS
        // ==========================================================

        public async Task<IActionResult> ActualizarEstados()
        {
            try
            {
                await _service
                    .ActualizarEstadosAutomaticosAsync();

                TempData["Success"] =
                    "Estados actualizados correctamente";
            }
            catch (Exception ex)
            {
                TempData["Error"] =
                    ex.Message;
            }

            return RedirectToAction(nameof(Listar));
        }


        // ==========================================================
        // HORARIOS OCUPADOS
        //
        // Utilizado por el modal de horarios.
        // ==========================================================

        [HttpGet]
        public async Task<IActionResult> HorariosOcupados(
            int idMedico,
            DateTime fecha)
        {
            var horarios =
                await _medicoservice
                    .ObtenerHorariosPorMedicoAsync(idMedico)
                ?? new List<Horario>();


            var ocupadas =
                await _context.Cita
                    .Include(c => c.Paciente)
                    .Where(c =>
                        c.Id_Medico == idMedico &&
                        c.Fecha.Date == fecha.Date)
                    .OrderBy(c => c.Hora)
                    .ToListAsync();


            var vm =
                new HorariosViewModel
                {
                    Horarios = horarios,
                    Ocupadas = ocupadas
                };


            return PartialView(
                "_ModalHorarios",
                vm);
        }


        // ==========================================================
        // BUSCAR CITA
        //
        // IMPORTANTE:
        // También se modifica para que una búsqueda por paciente
        // consulte TODAS las citas y no solamente PENDIENTES.
        // ==========================================================

        [HttpGet]
        public async Task<JsonResult> Buscar(
            string texto)
        {
            List<Cita> lista;


            texto = texto?.Trim();


            // =====================================================
            // SIN TEXTO
            //
            // Por defecto se mantienen solamente pendientes.
            // =====================================================

            if (string.IsNullOrWhiteSpace(texto))
            {
                lista =
                    await _service.ListarPorEstadoAsync(
                        EstadoCita.PENDIENTE
                    );

                lista =
                    lista
                        .OrderBy(c => c.Fecha)
                        .ThenBy(c => c.Hora)
                        .ToList();
            }

            // =====================================================
            // CON TEXTO
            //
            // Buscar en TODAS las citas.
            // =====================================================

            else
            {
                var todas =
                    await _service.ListarAsync();

                lista =
                    todas
                        .Where(c =>
                            c.Paciente != null &&
                            (
                                c.Paciente.Nombres.Contains(
                                    texto,
                                    StringComparison.OrdinalIgnoreCase)

                                ||

                                c.Paciente.Apellidos.Contains(
                                    texto,
                                    StringComparison.OrdinalIgnoreCase)

                                ||

                                $"{c.Paciente.Nombres} {c.Paciente.Apellidos}"
                                    .Contains(
                                        texto,
                                        StringComparison.OrdinalIgnoreCase)
                            )
                        )
                        // PENDIENTE SIEMPRE PRIMERO
                        .OrderBy(c =>
                            c.Estado == EstadoCita.PENDIENTE
                                ? 0
                                : 1)

                        .ThenBy(c => c.Fecha)

                        .ThenBy(c => c.Hora)

                        .ToList();
            }


            // =====================================================
            // RESPUESTA JSON
            // =====================================================

            var resultado =
                lista.Select(c => new
                {
                    idCita = c.Id_Cita,

                    fecha =
                        c.Fecha.ToString("dd/MM/yyyy"),

                    hora =
                        c.Hora,

                    motivo =
                        c.Motivo,

                    estado =
                        c.Estado.ToString(),

                    pacienteNombre =
                        c.Paciente != null
                            ? c.Paciente.Nombres
                            : "",

                    pacienteApellido =
                        c.Paciente != null
                            ? c.Paciente.Apellidos
                            : "",

                    medicoNombre =
                        c.Medico != null
                            ? c.Medico.Nombres
                            : "",

                    medicoApellido =
                        c.Medico != null
                            ? c.Medico.Apellidos
                            : ""
                });


            return Json(resultado);
        }


        // ==========================================================
        // HORARIOS
        // ==========================================================

        [HttpGet]
        public async Task<IActionResult> Horarios(
            int idMedico)
        {
            var horarios =
                await _medicoservice
                    .ObtenerHorariosPorMedicoAsync(
                        idMedico);

            var ocupadas =
                await _service.HorariosOcupadosAsync(
                    idMedico,
                    DateTime.Today);


            var vm =
                new HorariosViewModel
                {
                    Horarios = horarios,
                    Ocupadas = ocupadas
                };


            return PartialView(
                "_ModalHorarios",
                vm);
        }


        // ==========================================================
        // EDITAR - GET
        // ==========================================================

        [HttpGet]
        public async Task<IActionResult> Editar(
            int id)
        {
            var cita =
                await _service.ObtenerPorIdAsync(id);

            if (cita == null)
                return NotFound();


            // CARGAR DATOS PARA MODALES

            ViewBag.Pacientes =
                _pacienteservice.ListarTodosPaciente();

            ViewBag.Medicos =
                await _medicoservice.ListarTodosAsync();


            return View(cita);
        }


        // ==========================================================
        // EDITAR - POST
        // ==========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(Cita c)
        {
            try
            {
                // =========================================
                // VALIDAR HORARIO DEL MÉDICO
                // =========================================

                bool atiende =
                    await _service
                        .MedicoAtiendeEnHorarioAsync(
                            c.Id_Medico,
                            c.Fecha,
                            c.Hora
                        );


                if (!atiende)
                {
                    TempData["Error"] =
                        "El médico no atiende en ese horario";


                    // =========================================
                    // RECARGAR DATOS COMPLETOS
                    // =========================================

                    var citaOriginal =
                        await _service
                            .ObtenerPorIdAsync(
                                c.Id_Cita);

                    if (citaOriginal != null)
                    {
                        // SOLO ACTUALIZA FECHA Y HORA

                        citaOriginal.Fecha =
                            c.Fecha;

                        citaOriginal.Hora =
                            c.Hora;

                        citaOriginal.Motivo =
                            c.Motivo;
                    }


                    ViewBag.Pacientes =
                        _pacienteservice
                            .ListarTodosPaciente();

                    ViewBag.Medicos =
                        await _medicoservice
                            .ListarTodosAsync();


                    return View(citaOriginal);
                }


                // =========================================
                // ACTUALIZAR
                // =========================================

                var msg =
                    await _service
                        .ActualizarAsync(c);


                TempData[
                    msg == "OK"
                        ? "Success"
                        : "Error"
                ] =
                    msg == "OK"
                        ? "Cita actualizada correctamente"
                        : msg;


                return RedirectToAction(
                    nameof(Listar));
            }
            catch (Exception ex)
            {
                TempData["Error"] =
                    ex.Message;


                var citaOriginal =
                    await _service
                        .ObtenerPorIdAsync(
                            c.Id_Cita);

                if (citaOriginal != null)
                {
                    citaOriginal.Fecha =
                        c.Fecha;

                    citaOriginal.Hora =
                        c.Hora;

                    citaOriginal.Motivo =
                        c.Motivo;
                }


                ViewBag.Pacientes =
                    _pacienteservice
                        .ListarTodosPaciente();

                ViewBag.Medicos =
                    await _medicoservice
                        .ListarTodosAsync();


                return View(citaOriginal);
            }
        }


        // ==========================================================
        // AUDITORÍA
        // ==========================================================

        private async Task LogAsync(
            string accion,
            string modulo,
            string descripcion)
        {
            if (
                long.TryParse(
                    User.FindFirstValue("Id_Usuario"),
                    out var uid))
            {
                await _historial.RegistrarAsync(
                    uid,
                    accion,
                    modulo,
                    descripcion,
                    HttpContext
                        .Connection
                        .RemoteIpAddress?
                        .ToString()
                );
            }
        }
    }
}