# Watson Assistant / watsonx Assistant

La incorporación es opcional. Para este sistema es útil como asistente FAQ y orientación de procesos, pero no es requisito para la gestión transaccional de citas. La documentación oficial actual de IBM describe el uso de la API REST/SDK v2 para aplicaciones cliente y la gestión de estado de sesiones.

La recomendación es integrar Watson únicamente con información pública del flujo de atención y derivar cualquier operación sensible a la aplicación autenticada. No enviar DNI, pagos ni datos clínicos al asistente.

La carpeta `Integraciones/WatsonAssistant` contiene la configuración preparada y su guía. Las credenciales deben ir como secretos.
