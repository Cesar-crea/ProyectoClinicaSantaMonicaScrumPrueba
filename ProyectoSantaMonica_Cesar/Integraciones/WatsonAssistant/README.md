# Integración opcional con IBM watsonx Assistant

La funcionalidad se deja como **opcional** porque el proyecto puede operar sin chatbot. Para esta clínica sí tiene sentido como asistente FAQ para orientar sobre citas, horarios, cancelaciones y requisitos; no debe diagnosticar ni exponer datos clínicos.

IBM documenta que la API de ejecución v2 permite que una aplicación cliente envíe mensajes al asistente y mantenga el estado de la sesión. La documentación actual indica además requisitos de plan para el uso de la API v2. Por eso no se incluye una clave real ni se hace dependiente el arranque de la aplicación de Watson.

## Configuración propuesta

Variables/secretos: `WatsonAssistant__Enabled`, `WatsonAssistant__ServiceUrl`, `WatsonAssistant__ApiKey`, `WatsonAssistant__AssistantId`, `WatsonAssistant__EnvironmentId`. Nunca colocar la API key en Git.

## Casos de uso recomendados
- ¿Cómo solicito una cita?
- ¿Cómo cancelo una cita?
- ¿Qué datos necesito para registrar un paciente?
- ¿Cómo consulto disponibilidad?
- ¿Dónde encuentro un reporte?

## No debe hacer
- Diagnóstico médico.
- Interpretación de resultados clínicos.
- Consulta de historias clínicas.
- Mostrar DNI, pagos u otros datos privados de un paciente.

Fuente oficial: https://cloud.ibm.com/docs/watson-assistant
