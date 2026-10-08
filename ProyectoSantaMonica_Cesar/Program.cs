using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using ProyectoSantaMonica_Cesar.Data;
using ProyectoSantaMonica_Cesar.Models;
using ProyectoSantaMonica_Cesar.Repository;
using ProyectoSantaMonica_Cesar.Service.MedicoService;
using ProyectoSantaMonica_Cesar.Service.UsuarioService;
using ProyectoSantaMonica_Cesar.Service.EspecialidadService;
using ProyectoSantaMonica_Cesar.Service.HorarioService;
using ProyectoSantaMonica_Cesar.Service.PacienteService;
using ProyectoSantaMonica_Cesar.Service.CitaService;
using ProyectoSantaMonica_Cesar.Service.ComprobanteDePagoService;
using ProyectoSantaMonica_Cesar.Service.ReporteService;
using ProyectoSantaMonica_Cesar.Service.HistorialUsuarioService;
using QuestPDF.Infrastructure;
using Microsoft.AspNetCore.Identity;


var builder = WebApplication.CreateBuilder(args);

// PDF reports: configure once at application startup. Verify license eligibility before production use.
QuestPDF.Settings.License = LicenseType.Community;

/// AUTENTICACIÓN
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Usuario/Login";
        options.LogoutPath = "/Usuario/Logout";
        options.AccessDeniedPath = "/Usuario/AccesoDenegado";
    });

///  AUTORIZACIÓN
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("ADMINISTRADOR", p => p.RequireRole("ADMINISTRADOR"));
    options.AddPolicy("RECEPCIONISTA", p => p.RequireRole("RECEPCIONISTA"));
    options.AddPolicy("CAJERO", p => p.RequireRole("CAJERO"));
});

///  DB
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));
/// REPOSITORIOS 
builder.Services.AddScoped<UsuarioRepository>();
builder.Services.AddScoped<EspecialidadRepository>();
builder.Services.AddScoped<PacienteRepository>();
builder.Services.AddScoped<MedicoRepository>();
builder.Services.AddScoped<HorarioRepository>();
builder.Services.AddScoped<CitaRepository>();
builder.Services.AddScoped<ComprobanteDePagoRepository>();

///  SERVICIOS 
builder.Services.AddScoped<IUsuarioService, UsuarioService>();
builder.Services.AddScoped<IMedicoService,MedicoService>();
builder.Services.AddScoped<IEspecialidadService,EspecialidadService>();
builder.Services.AddScoped<IHorarioService, HorarioServiceImpl>();
builder.Services.AddScoped<IPacienteService, PacienteService>();
builder.Services.AddScoped<ICitaService, CitaService>();
builder.Services.AddScoped<IComprobanteDePagoService, ComprobanteDePagoService>();
builder.Services.AddScoped<IReporteService, ReporteService>();
builder.Services.AddScoped<IHistorialUsuarioService, HistorialUsuarioService>();
builder.Services.AddHttpClient();

///  MVC
builder.Services.AddControllersWithViews();

///  SESSION
builder.Services.AddSession();

var app = builder.Build();


///  PIPELINE
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

///  STATIC FILES
app.UseStaticFiles();

///  UPLOADS
var uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "uploads");

if (!Directory.Exists(uploadsPath))
{
    Directory.CreateDirectory(uploadsPath);
}

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(uploadsPath),
    RequestPath = "/uploads"
});

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.UseSession();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Usuario}/{action=Login}/{id?}");

app.Run();

public partial class Program { }
