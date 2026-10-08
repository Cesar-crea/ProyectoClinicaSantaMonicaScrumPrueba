# Requisitos del entorno

## Entorno recomendado para esta entrega

- Visual Studio 2022 17.12 o superior.
- .NET SDK 9.x. Esta entrega fija `9.0.101` en `global.json` y permite avanzar a la siguiente feature band 9.x mediante `rollForward: latestFeature`.
- Carga de trabajo de Visual Studio: **ASP.NET y desarrollo web**.
- SQL Server/LocalDB para la base de datos de Santa Monica.
- Navegador moderno.

## Comprobación rápida

Desde una terminal en la raíz de la solución:

```powershell
dotnet --version
dotnet --info
dotnet restore .\ProyectoSantaMonica_Cesar.sln
dotnet build .\ProyectoSantaMonica_Cesar.sln
dotnet test .\ProyectoSantaMonica_Cesar.sln --no-build
```

La máquina del usuario debe tener un SDK 9.x. `global.json` evita que el proyecto seleccione accidentalmente un SDK de otra versión principal.

## Base de datos

El ZIP incluye `Database/01_Additions.sql` con las tablas/índices agregados por esta entrega. El sistema original utiliza además procedimientos almacenados `sp_*` y la base `DB_SantaMonica`; esos objetos pertenecen a la base de datos original y no estaban incluidos como script completo en el ZIP de origen. Por ello, antes de ejecutar la aplicación con datos reales hay que restaurar/crear esa base original y ejecutar `01_Additions.sql`.

## Nota sobre IIS Express

IIS Express no es necesario para compilar el proyecto con `dotnet build` ni para ejecutarlo con `dotnet run`. Si Visual Studio Installer muestra un error al instalar `Microsoft.Ancm.IISExpress.Msi`, se puede utilizar el perfil de proyecto/Kestrel mientras se repara IIS Express.
