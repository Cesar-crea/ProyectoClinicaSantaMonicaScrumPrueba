# Plan de pruebas - Release 1.1

## Objetivo
Verificar funcionalidad, reglas de negocio, seguridad por rol, integridad de datos, generación de reportes, integración y regresión. El objetivo de cobertura funcional es 100% de criterios de aceptación críticos y una cobertura automatizada creciente sobre servicios/reglas.

## Niveles
1. Unitarias: validaciones, filtros y reglas puras.
2. Integración: servicio + EF Core + base de datos de prueba.
3. API/MVC: rutas, autorización, antiforgery y respuestas.
4. Sistema/UAT: flujos completos por rol.
5. Regresión: ejecutar suite completa después de cada cambio transversal.

## Tipos
- Positivas
- Negativas
- Seguridad/Integridad
- Concurrencia
- Exportación PDF/Excel
- Compatibilidad de navegador

## Datos de prueba
Usar únicamente datos ficticios. DNI de 8 dígitos, CMP ficticio, pacientes y médicos sintéticos.

## Exit criteria
- 100% de pruebas críticas PASS.
- ≥95% de pruebas totales PASS antes de UAT.
- 0 críticos/altos abiertos.
- Reportes verificados visualmente y por contenido.
- Concurrencia: no se permiten dos citas activas para médico/fecha/hora.

> Nota: el ZIP se entrega con la matriz y pruebas automatizadas preparadas. No se afirma una ejecución al 100% en este entorno porque aquí no está instalado el SDK `dotnet` ni SQL Server; deben ejecutarse en Windows/CI.
