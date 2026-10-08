using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ProyectoSantaMonica_Cesar.Service.HistorialUsuarioService;
using System.Security.Claims;
using ProyectoSantaMonica_Cesar.Models;
using ProyectoSantaMonica_Cesar.Repository;
using ProyectoSantaMonica_Cesar.Service.PacienteService;

namespace ProyectoSantaMonica_Cesar.Controllers
{
    [Authorize(Roles = "ADMINISTRADOR,RECEPCIONISTA")]
    public class PacienteController : Controller
    {
        private readonly IPacienteService _service;
        private readonly IHistorialUsuarioService _historial;

        public PacienteController(IPacienteService service, IHistorialUsuarioService historial)
        {
            _service = service;
            _historial = historial;
        }

        public IActionResult Index()
        {
            var lista = _service.ListarTodosPaciente();
            return View(lista);
        }

        public IActionResult Create()
        {
            return View(new Paciente());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Paciente paciente)
        {
            if (!ModelState.IsValid)
                return View(paciente);

            _service.GuardarPaciente(paciente);
            _ = LogAsync("REGISTRAR", "PACIENTES", $"Paciente DNI {paciente.Dni} registrado");
            TempData["mensaje"] = "Paciente registrado correctamente";
            return RedirectToAction("Index");
        }

        public IActionResult Edit(int id)
        {
            var paciente = _service.BuscarPacientePorId(id);
            return View(paciente);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Paciente paciente)
        {
            _service.ActualizarPaciente(paciente);
            _ = LogAsync("ACTUALIZAR", "PACIENTES", $"Paciente #{paciente.Id_Paciente} actualizado");
            TempData["mensaje"] = "Paciente actualizado";
            return RedirectToAction("Index");
        }

        public IActionResult Delete(int id)
        {
            _service.EliminarPaciente(id);
            TempData["mensaje"] = "Paciente eliminado";
            return RedirectToAction("Index");
        }

        [HttpGet]
        public JsonResult Buscar(string texto = "")
        {
            List<Paciente> lista;

            if (string.IsNullOrWhiteSpace(texto))
            {
                lista = _service.ListarTodosPaciente();
            }
            else
            {
                // ✅ AQUÍ ESTÁ EL CAMBIO
                lista = _service.BuscarGeneral(texto);
            }

            var data = lista.Select(p => new
            {
                id = p.Id_Paciente,
                nombres = p.Nombres,
                apellidos = p.Apellidos,
                dni = p.Dni
            });

            return Json(data);
        }

        private Task LogAsync(string accion, string modulo, string descripcion)
        {
            return long.TryParse(User.FindFirstValue("Id_Usuario"), out var uid)
                ? _historial.RegistrarAsync(uid, accion, modulo, descripcion, HttpContext.Connection.RemoteIpAddress?.ToString())
                : Task.CompletedTask;
        }

    }
}
