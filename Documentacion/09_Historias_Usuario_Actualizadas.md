# Historias de usuario actualizadas

## Base del documento fuente
Se conserva la jerarquía Nivel 0 a Nivel 3 y los cuatro sprints de dos semanas. La fuente contempla 3 HU de nivel 0, 7 de nivel 1, 8 de nivel 2 y 5 de nivel 3.

## Nuevas historias / extensión Release 1.1

### REP-HU01 - Reporte de citas
**Como:** Administrador o Recepcionista  
**Quiero:** generar un reporte de citas por periodo, médico y estado en PDF y Excel  
**Para:** consultar y entregar información operativa de la agenda.

**Aceptación:**
- Requiere autenticación.
- Admin y Recepcionista tienen acceso.
- Desde/Hasta son obligatorios y Desde <= Hasta.
- El resultado respeta los filtros.
- PDF y XLSX contienen los mismos registros.

### REP-HU02 - Reporte CMP
**Como:** Administrador  
**Quiero:** generar el reporte de médicos y CMP en PDF y Excel  
**Para:** disponer del padrón de colegiatura médica.

**Aceptación:**
- Solo rol ADMINISTRADOR.
- Incluye médico, DNI, CMP, especialidad y teléfono.
- PDF y XLSX contienen los mismos datos.
- Recepcionista y Cajero reciben acceso denegado.

### AUD-HU01 - Historial de usuario
**Como:** Administrador  
**Quiero:** consultar las acciones realizadas por los usuarios  
**Para:** mantener trazabilidad y facilitar auditoría.

**Aceptación:**
- Registra login/logout y operaciones relevantes.
- Incluye fecha/hora e IP cuando esté disponible.
- No sustituye la historia clínica.
- Solo Admin puede consultar el historial global.

### QA-HU01 - Calidad continua
**Como:** equipo Scrum  
**Quiero:** ejecutar pruebas automáticamente en cada PR  
**Para:** detectar regresiones antes de integrar.

**Aceptación:** build y test deben pasar para permitir merge a ramas protegidas.

## Definition of Done específica
- Código compilable.
- Validaciones servidor + cliente cuando aplique.
- Autorización por rol.
- Caso positivo y negativo.
- Prueba de integración cuando el cambio cruza capas.
- Evidencia documentada.
- Sin secretos en Git.
