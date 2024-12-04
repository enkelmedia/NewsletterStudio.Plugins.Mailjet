using System.Collections.Generic;

namespace NewsletterStudio.Plugins.Mailjet.Backoffice.Models;

public class CheckWebhookConfigurationResponse
{
    public required string WebhookUrl { get; set; }
    public bool IsBaseUrlLocalhost { get; set; }
}
