> **Versión corregida:** esta distribución usa **.NET 9** y está preparada para Visual Studio 2022 17.12+ con el SDK 9.x.

# Sistema Web de Gestión de Citas Médicas - Clínica Santa Mónica

## Mejoras incorporadas
- Reporte de citas para **Administrador y Recepcionista** en PDF y Excel.
- Reporte CMP exclusivo del **Administrador** en PDF y Excel.
- Historial/auditoría de usuario.
- Matriz de testing ampliada y plan de pruebas.
- Pruebas automatizadas de integración del módulo de reportes.
- Pipeline CI con GitHub Actions.
- Dockerfile para cloud.
- Guía opcional de IBM watsonx Assistant.
- Script SQL de soporte.

## Requisitos
- Visual Studio 2022 17.12 o superior.
- .NET SDK 9.x (la entrega incluye `global.json` fijado a 9.0.101).
- Carga de trabajo **ASP.NET y desarrollo web**.
- SQL Server/LocalDB.
- Navegador moderno.

## Ejecución
1. Revisar `appsettings.json` y colocar la cadena SQL local.
2. Ejecutar la BD/procedimientos existentes y `Database/01_Additions.sql`.
3. `dotnet restore ProyectoSantaMonica_Cesar.sln`
4. `dotnet build ProyectoSantaMonica_Cesar.sln`
5. `dotnet test ProyectoSantaMonica_Cesar.sln`
6. Ejecutar la aplicación.
7. Opcional: ejecutar `VERIFICAR_Y_COMPILAR.ps1` para comprobar SDK, restore, build y tests.

Usuario inicial de desarrollo: `admin1` / `1234` (cambiarlo inmediatamente en un ambiente real).

## Reportes
`Reportes` permite seleccionar periodo, médico y estado. Citas: Admin/Recepcionista. CMP: solo Admin.

## Importante
El documento fuente define Release 1.0 en ASP.NET Core MVC + SQL Server, cuatro sprints de dos semanas y 35 story points. Las mejoras de este ZIP se plantean como una extensión de calidad/Release 1.1.

## Corrección de restauración NuGet (NET 9)

- `Microsoft.Data.SqlClient` se fija en `5.1.6`, versión mínima requerida por `Microsoft.EntityFrameworkCore.SqlServer 9.0.0`, para evitar la degradación de paquete NU1605.
- Se eliminó la referencia directa a `Portable.BouncyCastle 1.7.0`, que no es utilizada por el código de la aplicación y produce una advertencia de compatibilidad con `net9.0`.
