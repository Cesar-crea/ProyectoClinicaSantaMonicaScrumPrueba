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


        // ==========================================================
        // LISTAR CITAS
        // ==========================================================

        [HttpGet]
        public async Task<IActionResult> Listar(
            string? texto,
            bool verTodas = false)
        {
            try
            {
                List<Cita> lista;

                await _service.ActualizarEstadosAutomaticosAsync();

                texto = texto?.Trim();


                // ==================================================
                // CON BÚSQUEDA
                // ==================================================

                if (!string.IsNullOrWhiteSpace(texto))
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
                            .OrderBy(c =>
                                c.Estado == EstadoCita.PENDIENTE
                                    ? 0
                                    : 1)
                            .ThenBy(c => c.Fecha)
                            .ThenBy(c => c.Hora)
                            .ToList();
                }


                // ==================================================
                // SIN BÚSQUEDA
                // SOLO PENDIENTES
                // ==================================================

                else if (!verTodas)
                {
                    lista =
                        await _service.ListarPorEstadoAsync(
                            EstadoCita.PENDIENTE);

                    lista =
                        lista
                            .OrderBy(c => c.Fecha)
                            .ThenBy(c => c.Hora)
                            .ToList();
                }


                // ==================================================
                // VER TODAS
                // ==================================================

                else
                {
                    lista =
                        await _service.ListarAsync();

                    lista =
                        lista
                            .OrderBy(c =>
                                c.Estado == EstadoCita.PENDIENTE
                                    ? 0
                                    : 1)
                            .ThenBy(c => c.Fecha)
                            .ThenBy(c => c.Hora)
                            .ToList();
                }


                ViewBag.Texto = texto;
                ViewBag.VerTodas = verTodas;

                return View(lista);
            }
            catch (Exception ex)
            {
                TempData["Error"] =
                    ex.Message;

                ViewBag.Texto = texto;
                ViewBag.VerTodas = verTodas;

                return View(
                    new List<Cita>()
                );
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

            var cita =
                new Cita
                {
                    Fecha = DateTime.Today,
                    Estado = EstadoCita.PENDIENTE
                };

            return View(cita);
        }


        // ==========================================================
        // REGISTRAR - POST
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

                c.Fecha =
                    c.Fecha.Date;


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
                // USUARIO
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
                // MEDICO
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
                // PACIENTE
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
                // HORA
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
                // VALIDAR HORARIO
                // ==================================================

                var motivoHorario =
                    await _service.ValidarHorarioCitaAsync(
                        c.Id_Medico,
                        c.Fecha,
                        c.Hora);


                if (motivoHorario != null)
                {
                    TempData["Error"] =
                        motivoHorario switch
                        {
                            "MEDICO_NO_ATIENDE_DIA" =>
                                "El médico no tiene horario de atención para el día seleccionado.",

                            "HORA_FUERA_HORARIO" =>
                                "La hora seleccionada está fuera del horario de atención del médico.",

                            "HORA_YA_PASO" =>
                                "La hora seleccionada ya pasó. Debe seleccionar una hora futura.",

                            "HORA_OCUPADA" =>
                                "La hora seleccionada ya está ocupada. Seleccione otra hora disponible.",

                            _ =>
                                "La hora seleccionada no está disponible."
                        };


                    ViewBag.Pacientes =
                        _pacienteservice.ListarTodosPaciente();

                    ViewBag.Medicos =
                        await _medicoservice.ListarTodosAsync();

                    return View(c);
                }


                // ==================================================
                // REGISTRAR
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

                    return RedirectToAction(
                        nameof(Listar)
                    );
                }


                TempData["Error"] =
                    msg;

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
        // HORAS DISPONIBLES
        // ==========================================================

        [HttpGet]
        public async Task<IActionResult> HorasDisponibles(
            int idMedico,
            DateTime fecha,
            int idCita = 0)
        {
            try
            {
                // ==================================================
                // VALIDAR MEDICO
                // ==================================================

                if (idMedico <= 0)
                {
                    return Json(new
                    {
                        ok = false,
                        mensaje = "Debe seleccionar un médico."
                    });
                }


                // ==================================================
                // NORMALIZAR FECHA
                // ==================================================

                fecha =
                    fecha.Date;


                // ==================================================
                // FECHA ANTERIOR
                // ==================================================

                if (fecha < DateTime.Today)
                {
                    return Json(new
                    {
                        ok = false,
                        mensaje =
                            "No se pueden consultar fechas anteriores a hoy."
                    });
                }


                // ==================================================
                // DIA DE SEMANA
                // ==================================================

                var diaSemana =
                    DIaSemanaHelper.FromDateTime(fecha);


                // ==================================================
                // HORARIOS DEL MEDICO
                // ==================================================

                var horarios =
                    await _medicoservice
                        .ObtenerHorariosPorMedicoAsync(
                            idMedico);


                var horariosDelDia =
                    horarios
                        .Where(h =>
                            h.Dia_Semana.HasValue &&
                            h.Dia_Semana.Value == diaSemana &&
                            h.Horario_Entrada.HasValue &&
                            h.Horario_Salida.HasValue)
                        .OrderBy(h =>
                            h.Horario_Entrada)
                        .ToList();


                // ==================================================
                // SIN HORARIO
                // ==================================================

                if (!horariosDelDia.Any())
                {
                    return Json(new
                    {
                        ok = true,

                        mensaje =
                            "El médico no tiene horario de atención para este día.",

                        horas =
                            Array.Empty<string>()
                    });
                }


                // ==================================================
                // CITAS OCUPADAS
                //
                // Al editar:
                // excluir la propia cita.
                // ==================================================

                HashSet<TimeSpan> horasOcupadas;


                if (idCita > 0)
                {
                    horasOcupadas =
                        await _context.Cita
                            .Where(c =>
                                c.Id_Medico == idMedico &&
                                c.Fecha.Date == fecha.Date &&
                                c.Id_Cita != idCita &&
                                c.Estado != EstadoCita.CANCELADO)
                            .Select(c => c.Hora)
                            .ToHashSetAsync();
                }
                else
                {
                    var ocupadas =
                        await _service.HorariosOcupadosAsync(
                            idMedico,
                            fecha);


                    horasOcupadas =
                        ocupadas
                            .Where(x =>
                                x.Estado != EstadoCita.CANCELADO)
                            .Select(x =>
                                x.Hora)
                            .ToHashSet();
                }


                // ==================================================
                // GENERAR HORAS
                // ==================================================

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
                        hora = hora.Add(
                            TimeSpan.FromHours(1)))
                    {
                        // ------------------------------------------
                        // HORAS PASADAS
                        // ------------------------------------------

                        if (
                            fecha.Date == DateTime.Today &&
                            hora <= DateTime.Now.TimeOfDay)
                        {
                            continue;
                        }


                        // ------------------------------------------
                        // HORAS OCUPADAS
                        // ------------------------------------------

                        if (!horasOcupadas.Contains(hora))
                        {
                            horasDisponibles.Add(
                                hora.ToString(@"hh\:mm")
                            );
                        }
                    }
                }


                // ==================================================
                // ORDENAR / ELIMINAR DUPLICADOS
                // ==================================================

                horasDisponibles =
                    horasDisponibles
                        .Distinct()
                        .OrderBy(x => x)
                        .ToList();


                // ==================================================
                // MENSAJE
                // ==================================================

                var mensaje =
                    horasDisponibles.Any()
                        ? ""
                        : fecha.Date == DateTime.Today
                            ? "No hay horas disponibles para hoy. Las horas de atención ya pasaron o están ocupadas."
                            : "No hay horas disponibles para esta fecha. Las horas de atención pueden estar ocupadas.";


                // ==================================================
                // RESPUESTA
                // ==================================================

                return Json(new
                {
                    ok = true,
                    mensaje,
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


            return RedirectToAction(
                nameof(Listar)
            );
        }


        // ==========================================================
        // DESACTIVAR
        // ==========================================================

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

                    return RedirectToAction(
                        nameof(Listar)
                    );
                }


                if (cita.Estado != EstadoCita.PENDIENTE)
                {
                    TempData["Error"] =
                        "Solo se pueden desactivar citas pendientes.";

                    return RedirectToAction(
                        nameof(Listar)
                    );
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
                        ? "Cita desactivada correctamente"
                        : msg;
            }
            catch (Exception ex)
            {
                TempData["Error"] =
                    ex.Message;
            }


            await LogAsync(
                "DESACTIVAR",
                "CITAS",
                $"Cita #{id} desactivada"
            );


            return RedirectToAction(
                nameof(Listar)
            );
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

                    return RedirectToAction(
                        nameof(Listar)
                    );
                }


                return View(cita);
            }
            catch (Exception ex)
            {
                TempData["Error"] =
                    ex.Message;

                return RedirectToAction(
                    nameof(Listar)
                );
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

                    return RedirectToAction(
                        nameof(Listar)
                    );
                }


                if (cita.Estado != EstadoCita.PENDIENTE)
                {
                    TempData["Error"] =
                        "Solo se pueden cancelar citas pendientes";

                    return RedirectToAction(
                        nameof(Listar)
                    );
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


            return RedirectToAction(
                nameof(Listar)
            );
        }


        // ==========================================================
        // FILTRAR POR ESTADO
        // ==========================================================

        public async Task<IActionResult> PorEstado(
            EstadoCita estado)
        {
            var lista =
                await _service.ListarPorEstadoAsync(
                    estado
                );

            return View(
                "Listar",
                lista
            );
        }


        // ==========================================================
        // PENDIENTES POR PACIENTE
        // ==========================================================

        public async Task<IActionResult> Pendientes(
            string dato)
        {
            var lista =
                await _service.PendientesPorPacienteAsync(
                    dato
                );

            return View(
                "Listar",
                lista
            );
        }


        // ==========================================================
        // ACTUALIZAR ESTADOS AUTOMATICOS
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


            return RedirectToAction(
                nameof(Listar)
            );
        }


        // ==========================================================
        // HORARIOS OCUPADOS
        //
        // UTILIZADO POR:
        // Ver horarios
        // ==========================================================

        [HttpGet]
        public async Task<IActionResult> HorariosOcupados(
            int idMedico,
            DateTime fecha)
        {
            try
            {
                // ==================================================
                // VALIDAR MEDICO
                // ==================================================

                if (idMedico <= 0)
                {
                    return BadRequest(
                        "Debe seleccionar un médico."
                    );
                }


                // ==================================================
                // NORMALIZAR FECHA
                // ==================================================

                fecha =
                    fecha.Date;


                // ==================================================
                // HORARIOS DEL MEDICO
                // ==================================================

                var horarios =
                    await _medicoservice
                        .ObtenerHorariosPorMedicoAsync(
                            idMedico)
                    ?? new List<Horario>();


                // ==================================================
                // CITAS OCUPADAS
                //
                // CANCELADO NO BLOQUEA EL HORARIO
                // ==================================================

                var ocupadas =
                    await _context.Cita
                        .Include(c => c.Paciente)
                        .Where(c =>
                            c.Id_Medico == idMedico &&
                            c.Fecha.Date == fecha.Date &&
                            c.Estado != EstadoCita.CANCELADO)
                        .OrderBy(c => c.Hora)
                        .ToListAsync();


                // ==================================================
                // VIEW MODEL
                // ==================================================

                var vm =
                    new HorariosViewModel
                    {
                        Horarios = horarios,
                        Ocupadas = ocupadas
                    };


                // ==================================================
                // PARTIAL
                // ==================================================

                return PartialView(
                    "_ModalHorarios",
                    vm
                );
            }
            catch (Exception ex)
            {
                return StatusCode(
                    500,
                    ex.Message
                );
            }
        }


        // ==========================================================
        // BUSCAR CITA
        // ==========================================================

        [HttpGet]
        public async Task<JsonResult> Buscar(
            string texto)
        {
            List<Cita> lista;


            texto =
                texto?.Trim();


            // ==================================================
            // SIN TEXTO
            // ==================================================

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


            // ==================================================
            // CON TEXTO
            // ==================================================

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
                        .OrderBy(c =>
                            c.Estado == EstadoCita.PENDIENTE
                                ? 0
                                : 1)
                        .ThenBy(c => c.Fecha)
                        .ThenBy(c => c.Hora)
                        .ToList();
            }


            // ==================================================
            // JSON
            // ==================================================

            var resultado =
                lista.Select(c => new
                {
                    idCita =
                        c.Id_Cita,

                    fecha =
                        c.Fecha.ToString(
                            "dd/MM/yyyy"
                        ),

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
                    DateTime.Today
                );


            var vm =
                new HorariosViewModel
                {
                    Horarios = horarios,
                    Ocupadas = ocupadas
                };


            return PartialView(
                "_ModalHorarios",
                vm
            );
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
            {
                return NotFound();
            }


            // ==================================================
            // DATOS PARA MODALES
            // ==================================================

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
        public async Task<IActionResult> Editar(
            Cita c)
        {
            try
            {
                // ==================================================
                // NORMALIZAR FECHA
                // ==================================================

                c.Fecha =
                    c.Fecha.Date;


                // ==================================================
                // VALIDAR HORARIO
                //
                // IMPORTANTE:
                // Se pasa c.Id_Cita para excluir la propia cita.
                // ==================================================

                var motivoHorario =
                    await _service.ValidarHorarioCitaAsync(
                        c.Id_Medico,
                        c.Fecha,
                        c.Hora,
                        c.Id_Cita
                    );


                if (motivoHorario != null)
                {
                    TempData["Error"] =
                        motivoHorario switch
                        {
                            "MEDICO_NO_ATIENDE_DIA" =>
                                "El médico no tiene horario de atención para el día seleccionado.",

                            "HORA_FUERA_HORARIO" =>
                                "La hora seleccionada está fuera del horario de atención del médico.",

                            "HORA_YA_PASO" =>
                                "La hora seleccionada ya pasó. Debe seleccionar una hora futura.",

                            "HORA_OCUPADA" =>
                                "La hora seleccionada ya está ocupada. Seleccione otra hora disponible.",

                            _ =>
                                "La hora seleccionada no está disponible."
                        };


                    // ==================================================
                    // RECUPERAR CITA ORIGINAL
                    // ==================================================

                    var citaOriginal =
                        await _service
                            .ObtenerPorIdAsync(
                                c.Id_Cita
                            );


                    if (citaOriginal != null)
                    {
                        citaOriginal.Fecha =
                            c.Fecha;

                        citaOriginal.Hora =
                            c.Hora;

                        citaOriginal.Motivo =
                            c.Motivo;
                    }


                    // ==================================================
                    // RECARGAR MODALES
                    // ==================================================

                    ViewBag.Pacientes =
                        _pacienteservice
                            .ListarTodosPaciente();


                    ViewBag.Medicos =
                        await _medicoservice
                            .ListarTodosAsync();


                    return View(
                        citaOriginal
                    );
                }


                // ==================================================
                // ACTUALIZAR
                // ==================================================

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
                    nameof(Listar)
                );
            }
            catch (Exception ex)
            {
                TempData["Error"] =
                    ex.Message;


                // ==================================================
                // RECUPERAR CITA ORIGINAL
                // ==================================================

                var citaOriginal =
                    await _service
                        .ObtenerPorIdAsync(
                            c.Id_Cita
                        );


                if (citaOriginal != null)
                {
                    citaOriginal.Fecha =
                        c.Fecha;

                    citaOriginal.Hora =
                        c.Hora;

                    citaOriginal.Motivo =
                        c.Motivo;
                }


                // ==================================================
                // RECARGAR DATOS
                // ==================================================

                ViewBag.Pacientes =
                    _pacienteservice
                        .ListarTodosPaciente();


                ViewBag.Medicos =
                    await _medicoservice
                        .ListarTodosAsync();


                return View(
                    citaOriginal
                );
            }
        }


        // ==========================================================
        // AUDITORIA
        // ==========================================================

        private async Task LogAsync(
            string accion,
            string modulo,
            string descripcion)
        {
            if (
                long.TryParse(
                    User.FindFirstValue(
                        "Id_Usuario"
                    ),
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