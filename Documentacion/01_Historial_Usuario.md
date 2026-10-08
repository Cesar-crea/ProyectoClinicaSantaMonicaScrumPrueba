# Historial del usuario / auditoría

Se incorpora una bitácora `Historial_Usuario` para trazabilidad de acciones autenticadas. Registra usuario, acción, módulo, descripción, fecha/hora e IP.

## Acciones mínimas
- LOGIN / LOGOUT
- REGISTRAR / ACTUALIZAR / ELIMINAR / CANCELAR en citas y pacientes
- REGISTRAR pagos
- GENERAR_PDF / GENERAR_EXCEL en reportes

## Acceso
Solo el Administrador puede consultar el historial completo. La tabla no debe usarse como historia clínica: el alcance del Release 1.0 sigue excluyendo historia clínica electrónica.
