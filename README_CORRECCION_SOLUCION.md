# Corrección de apertura de la solución

## Qué se corrigió

Esta versión fue preparada para Visual Studio 2022/Visual Studio compatible con .NET 8 y contiene una sola solución:

`ProyectoSantaMonica_Cesar.sln`

La solución contiene:
- `ProyectoSantaMonica_Cesar/ProyectoSantaMonica_Cesar.csproj`
- `ProyectoSantaMonica_Cesar.Tests/ProyectoSantaMonica_Cesar.Tests.csproj`

Además, ambos proyectos fueron actualizados de `net7.0` a `net9.0`, y las referencias de Entity Framework Core / ASP.NET Identity se actualizaron a la línea 9.0.

## Requisito importante

En Visual Studio Installer debe estar instalado:

- Visual Studio 2022 con una versión que soporte .NET 8.
- Carga de trabajo **ASP.NET y desarrollo web**.
- SDK de **.NET 8**.

Si Visual Studio indica que no reconoce `net9.0`, abra Visual Studio Installer → Modificar → ASP.NET y desarrollo web y verifique el SDK de .NET 9. Esta entrega usa Kestrel (perfiles `http`/`https`) y no depende del perfil IIS Express.

## Cómo abrirlo correctamente

1. Descomprima todo el ZIP en una carpeta local, por ejemplo `C:\Proyectos\ProyectoSantaMonica_Cesar`.
2. No abra un `.csproj` individual.
3. Abra únicamente `ProyectoSantaMonica_Cesar.sln`.
4. Espere la restauración de NuGet.
5. En el Explorador de soluciones debe aparecer el árbol de archivos y carpetas de ambos proyectos.

## Si Windows bloqueó los archivos descargados

Después de descomprimir, si Visual Studio continúa mostrando proyectos como **Descargado**, cierre Visual Studio y quite el bloqueo del archivo ZIP descargado desde Propiedades → Desbloquear antes de volver a extraerlo. También puede volver a extraer el ZIP en una carpeta local después de desbloquear el ZIP.

## Base de datos

El proyecto sigue necesitando SQL Server y la base de datos/procedimientos almacenados originales para ejecutar todas las funciones de negocio. El archivo `Database/01_Additions.sql` contiene las adiciones realizadas para historial e integridad.
