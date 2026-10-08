# Despliegue cloud

## Opción recomendada: Azure App Service + Azure SQL

Microsoft documenta el despliegue de ASP.NET Core con Azure App Service y Azure SQL Database y permite iniciar con una cuenta Azure gratuita. El proyecto incluye `Dockerfile` y configuración preparada para externalizar la cadena de conexión.

### Pasos
1. Crear cuenta Azure gratuita.
2. Crear Azure SQL Database.
3. Ejecutar el esquema/procedimientos actuales y `Database/01_Additions.sql`.
4. Crear App Service.
5. Configurar `ConnectionStrings__CibertecConnection` en Configuration.
6. No publicar `appsettings.Production.json` con credenciales.
7. Conectar GitHub Actions o publicar desde Visual Studio.
8. Verificar `/Usuario/Login`.
9. Ejecutar smoke/regresión.

### URL
Azure genera una URL tipo `https://<app>.azurewebsites.net`. **No puedo entregar un link público real desde este ZIP porque requiere una cuenta/suscripción cloud del propietario y credenciales que no deben compartirse en el chat.** El proyecto queda preparado para ese despliegue.
