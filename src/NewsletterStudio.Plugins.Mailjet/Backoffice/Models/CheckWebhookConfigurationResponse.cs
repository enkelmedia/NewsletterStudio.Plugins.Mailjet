using System.Collections.Generic;

namespace NewsletterStudio.Plugins.Mailjet.Backoffice.Models
{
    public class CheckWebhookConfigurationResponse
    {
        public required string WebhookUrl { get; set; }
        public bool IsBaseUrlLocalhost { get; set; }
    }

    public class CheckWebhookConfigurationRequest
    {
        public Dictionary<string, object> Settings { get; set; } = default!;
        public required Guid WorkspaceKey { get; set; }
        public required string BaseUrl { get; set; }
    }
}
