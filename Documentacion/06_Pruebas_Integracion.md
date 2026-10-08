# Pruebas de integración

El proyecto incorpora `ProyectoSantaMonica_Cesar.Tests` con EF Core InMemory para validar integración del servicio de reportes con el modelo de datos, filtros, CMP y generación de PDF/Excel.

### Extensión recomendada
Para una prueba de integración completa contra SQL Server, usar una BD `DB_SantaMonica_Test`, ejecutar el script de esquema/procedimientos y correr la suite con `ConnectionStrings__CibertecConnection` apuntando a esa BD. Nunca usar producción.

### Casos críticos
- Cita filtrada por periodo/estado.
- CMP por médico/especialidad.
- Fechas inválidas.
- Exportación PDF.
- Exportación XLSX.
- Concurrencia de citas mediante índice único SQL.
