using System.Collections.Generic;

namespace NewsletterStudio.Plugins.Mailjet.Backoffice.Models
{
    public class CheckWebhookConfigurationResponse
    {
        public string WebhookUrl { get; set; }
        public bool IsBaseUrlLocalhost { get; set; }
    }

    public class CheckWebhookConfigurationRequest
    {
        public Dictionary<string,object> Settings { get; set; }

        public string Test { get; set; }

        public string WorkspaceKey { get; set; }
        public string BaseUrl { get; set; }

    }
}