# Plan de lanzamiento Release 1.1

Basado en el Release 1.0 del documento fuente: cuatro sprints de dos semanas y 35 story points. Se conserva la estructura funcional y se agrega una capa de calidad, reportes y auditoría.

| Sprint | Semanas | Alcance | QA/Testing |
|---|---:|---|---|
| 1 | 1-2 | Autenticación, roles, especialidades, médicos, pacientes | Validaciones, permisos, duplicidad |
| 2 | 3-4 | Horarios y motor de citas | Concurrencia, transacciones, fechas/horas |
| 3 | 5-6 | Cancelación, historial, trazabilidad, notificaciones | Regresión, auditoría, flujos alternos |
| 4 | 7-8 | Pagos, reportes PDF/Excel, CMP | Integración, seguridad, exportaciones |
| 5 | 9 | CI/CD, hardening, nube, UAT | Smoke, regresión final, evidencia |

### Criterios Go/No-Go
1. Build Release sin errores.
2. Pruebas automatizadas verdes.
3. 100% de historias críticas con criterios de aceptación verificados.
4. 0 defectos críticos/altos abiertos.
5. Reportes de citas funcionan para Admin y Recepcionista.
6. Reporte CMP funciona únicamente para Admin.
7. SQL Server productivo configurado por secreto/variable, nunca hardcodeado.
