# Matriz de pruebas

La fuente define 46 pruebas en 4 sprints: Sprint 1=16, Sprint 2=8, Sprint 3=11, Sprint 4=11.

| ID | Sprint | Categoría | Caso | Resultado esperado |
|---|---:|---|---|---|
| T01 | 1 | Positiva | Login administrador válido | Dashboard Admin |
| T02 | 1 | Negativa | Login con contraseña incorrecta | Error sin sesión |
| T03 | 1 | Seguridad | Recepcionista intenta Admin | 403/AccessDenied |
| T04 | 1 | Positiva | Registrar paciente válido | Guardado |
| T05 | 1 | Negativa | DNI no numérico | Validación |
| T06 | 1 | Negativa | DNI duplicado | Rechazo |
| T07 | 1 | Negativa | Email inválido | Validación |
| T08 | 1 | Seguridad | Usuario deshabilitado | No permite acceso |
| T09 | 1 | Positiva | Registrar especialidad | Guardado |
| T10 | 1 | Negativa | Especialidad duplicada | Rechazo |
| T11 | 1 | Positiva | Registrar médico con CMP | Guardado |
| T12 | 1 | Negativa | CMP duplicado | Rechazo |
| T13 | 1 | Negativa | Campos obligatorios vacíos | Validación |
| T14 | 1 | Seguridad | Acceso sin autenticación | Redirección Login |
| T15 | 1 | Positiva | Buscar paciente por DNI | Ficha correcta |
| T16 | 1 | Integridad | Datos no válidos no persisten | BD sin cambio |
| T17 | 2 | Positiva | Crear horario válido | Disponible |
| T18 | 2 | Negativa | Salida <= entrada | Rechazo |
| T19 | 2 | Negativa | Horario duplicado | Rechazo |
| T20 | 2 | Positiva | Consultar disponibilidad | Solo disponibles |
| T21 | 2 | Positiva | Agendar cita disponible | Cita creada |
| T22 | 2 | Negativa | Agendar fuera de horario | Rechazo |
| T23 | 2 | Integridad | Dos reservas mismo slot | Solo una persiste |
| T24 | 2 | Integridad | Fallo transacción | Sin cita ni cambio parcial |
| T25 | 3 | Positiva | Cancelar cita pendiente | Cancelada y slot liberado |
| T26 | 3 | Negativa | Cancelar cita atendida/pagada | Rechazo |
| T27 | 3 | Positiva | Reprogramar | Historial conserva anterior |
| T28 | 3 | Positiva | Registrar notificación | Evidencia en historial |
| T29 | 3 | Negativa | Canal no disponible | Pendiente/reintento |
| T30 | 3 | Seguridad | Acceso a historial sin Admin | Rechazo |
| T31 | 3 | Integridad | Cancelación no elimina registro | Historial intacto |
| T32 | 3 | Regresión | Buscar citas pendientes | Resultados correctos |
| T33 | 3 | Negativa | Fecha pasada | Rechazo |
| T34 | 3 | Seguridad | Alterar Id_Usuario manualmente | Ignorado/validado servidor |
| T35 | 3 | Seguridad | CSRF en POST | Rechazo |
| T36 | 4 | Positiva | Registrar pago | Estado actualizado |
| T37 | 4 | Negativa | Pago monto <=0 | Rechazo |
| T38 | 4 | Negativa | Pago cita cancelada | Rechazo |
| T39 | 4 | Integridad | Segundo pago misma cita | Rechazo |
| T40 | 4 | Positiva | Comprobante detalle | Datos completos |
| T41 | 4 | Seguridad | Recepcionista accede a CMP | Rechazo |
| T42 | 4 | Positiva | Reporte citas PDF | PDF válido |
| T43 | 4 | Positiva | Reporte citas Excel | XLSX válido |
| T44 | 4 | Positiva | CMP PDF Admin | PDF válido |
| T45 | 4 | Positiva | CMP Excel Admin | XLSX válido |
| T46 | 4 | Seguridad | CMP con rol no Admin | Rechazo |
| T47 | 5 | CI | Restore/build | Verde |
| T48 | 5 | CI | Tests automáticos | Verde |
| T49 | 5 | CI | Artifact de resultados | Publicado |
| T50 | 5 | Seguridad | Secretos fuera del repo | Cumple |
| T51 | 5 | Cloud | App inicia con connection string | OK |
| T52 | 5 | Cloud | HTTPS | OK |
| T53 | 5 | Cloud | Logs de error | Disponibles |
| T54 | 5 | Regresión | Flujo login->cita->pago | PASS |
| T55 | 5 | Regresión | Reportes después de datos nuevos | Consistentes |
| T56 | 5 | UAT | Admin valida alcance | Aprobado |
