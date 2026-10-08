namespace ProyectoSantaMonica_Cesar.Integraciones.WatsonAssistant
{
    public class WatsonAssistantOptions
    {
        public bool Enabled { get; set; }
        public string ServiceUrl { get; set; } = string.Empty;
        public string ApiKey { get; set; } = string.Empty;
        public string AssistantId { get; set; } = string.Empty;
        public string EnvironmentId { get; set; } = "draft";
        public string Version { get; set; } = "2024-08-25";
    }
}
