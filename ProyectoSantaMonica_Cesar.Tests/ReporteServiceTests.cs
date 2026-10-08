
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using QuestPDF.Infrastructure;

using Microsoft.EntityFrameworkCore;
using ProyectoSantaMonica_Cesar.Data;
using ProyectoSantaMonica_Cesar.Models;
using ProyectoSantaMonica_Cesar.Models.enums;
using ProyectoSantaMonica_Cesar.Models.ViewModels;
using ProyectoSantaMonica_Cesar.Service.ReporteService;

namespace ProyectoSantaMonica_Cesar.Tests;

public class ReporteServiceTests
{
    static ReporteServiceTests()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    private static ApplicationDbContext Db()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }


    // ==========================================================
    // TEST 1
    // Obtener citas filtradas por período y estado
    // ==========================================================

    [Fact]
    public async Task ObtenerCitasAsync_FiltraPorPeriodoYEstado()
    {
        await using var db = Db();

        var esp = new Especialidad
        {
            Id_Especialidad = 1,
            Nombre = "Cardiología"
        };

        var medico = new Medico
        {
            Id_Medico = 1,
            Nombres = "Ana",
            Apellidos = "Médico",
            Dni = "12345678",
            Nro_Colegiatura = "CMP001",
            Telefono = "999999999",
            Id_Especialidad = 1,
            especialidad = esp
        };

        var paciente = new Paciente
        {
            Id_Paciente = 1,
            Nombres = "Juan",
            Apellidos = "Pérez",
            Dni = "87654321"
        };

        db.Especialidad.Add(esp);
        db.Medico.Add(medico);
        db.Paciente.Add(paciente);

        db.Cita.Add(new Cita
        {
            Id_Cita = 1,
            Id_Medico = 1,
            Id_Paciente = 1,
            Id_Usuario = 1,
            Fecha = DateTime.Today,
            Hora = new TimeSpan(10, 0, 0),
            Estado = EstadoCita.PENDIENTE,
            Medico = medico,
            Paciente = paciente
        });

        db.Cita.Add(new Cita
        {
            Id_Cita = 2,
            Id_Medico = 1,
            Id_Paciente = 1,
            Id_Usuario = 1,
            Fecha = DateTime.Today.AddDays(-40),
            Hora = new TimeSpan(10, 0, 0),
            Estado = EstadoCita.CANCELADO,
            Medico = medico,
            Paciente = paciente
        });

        await db.SaveChangesAsync();

        var service = new ReporteService(db);

        var rows = await service.ObtenerCitasAsync(
            new ReporteFiltrosViewModel
            {
                Desde = DateTime.Today.AddDays(-1),
                Hasta = DateTime.Today,
                Estado = "PENDIENTE"
            });

        Assert.Single(rows);
        Assert.Equal(1, rows[0].Id_Cita);
    }


    // ==========================================================
    // TEST 2
    // Obtener CMP, colegiatura y especialidad
    // ==========================================================

    [Fact]
    public async Task ObtenerCmpAsync_DevuelveColegiaturaYEspecialidad()
    {
        await using var db = Db();

        var esp = new Especialidad
        {
            Id_Especialidad = 1,
            Nombre = "Pediatría"
        };

        db.Especialidad.Add(esp);

        db.Medico.Add(new Medico
        {
            Id_Medico = 1,
            Nombres = "Luis",
            Apellidos = "García",
            Dni = "11111111",
            Nro_Colegiatura = "CMP999",
            Telefono = "988888888",
            Id_Especialidad = 1,
            especialidad = esp
        });

        await db.SaveChangesAsync();

        var rows =
            await new ReporteService(db).ObtenerCmpAsync();

        Assert.Single(rows);

        Assert.Equal(
            "CMP999",
            rows[0].CMP);

        Assert.Equal(
            "Pediatría",
            rows[0].Especialidad);
    }


    // ==========================================================
    // TEST 3
    // Rechazar período inválido
    // ==========================================================

    [Fact]
    public async Task ObtenerCitasAsync_RechazaPeriodoInvalido()
    {
        await using var db = Db();

        var service = new ReporteService(db);

        await Assert.ThrowsAsync<ArgumentException>(
            () => service.ObtenerCitasAsync(
                new ReporteFiltrosViewModel
                {
                    Desde = DateTime.Today,
                    Hasta = DateTime.Today.AddDays(-1)
                }));
    }


    // ==========================================================
    // TEST 4
    // Generación de Excel y PDF
    // ==========================================================

    [Fact]
    public async Task GenerarExcelYPdf_DevuelveArchivosNoVacios()
    {
        await using var db = Db();

        var service = new ReporteService(db);

        var filtros =
            new ReporteFiltrosViewModel();

        var rows =
            new List<ReporteCitaRow>();

        var excel =
            service.GenerarCitasExcel(rows, filtros);

        var pdf =
            service.GenerarCitasPdf(rows, filtros);

        Assert.True(excel.Length > 100);
        Assert.True(pdf.Length > 100);

        // PDF debe comenzar con %PDF
        Assert.Equal(0x25, pdf[0]); // %
        Assert.Equal(0x50, pdf[1]); // P
        Assert.Equal(0x44, pdf[2]); // D
        Assert.Equal(0x46, pdf[3]); // F
    }
}


// ==========================================================
// TESTS DE AUTORIZACIÓN
// ==========================================================

public class AuthorizationContractTests
{
    [Fact]
    public void ReportesController_AllowsAdminAndRecepcionista()
    {
        var attr =
            typeof(
                ProyectoSantaMonica_Cesar.Controllers.ReportesController)
            .GetCustomAttributes(
                typeof(
                    Microsoft.AspNetCore.Authorization.AuthorizeAttribute),
                true)
            .Cast<
                Microsoft.AspNetCore.Authorization.AuthorizeAttribute>()
            .Single();

        Assert.Contains("ADMINISTRADOR", attr.Roles);
        Assert.Contains("RECEPCIONISTA", attr.Roles);
    }


    [Fact]
    public void CmpEndpoints_AreAdminOnly()
    {
        var methods =
            new[]
            {
                "CmpPdf",
                "CmpExcel",
                "Historial"
            };

        foreach (var name in methods)
        {
            var method =
                typeof(
                    ProyectoSantaMonica_Cesar.Controllers.ReportesController)
                .GetMethod(name)!;

            var attr =
                method
                    .GetCustomAttributes(
                        typeof(
                            Microsoft.AspNetCore.Authorization.AuthorizeAttribute),
                        true)
                    .Cast<
                        Microsoft.AspNetCore.Authorization.AuthorizeAttribute>()
                    .Single();

            Assert.Equal(
                "ADMINISTRADOR",
                attr.Roles);
        }
    }
}

