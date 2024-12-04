namespace NewsletterStudio.Plugins.Mailjet.Backoffice.Models;

public class CheckWebhookConfigurationRequest
{
    public Dictionary<string, object> Settings { get; set; } = default!;
    public required Guid WorkspaceKey { get; set; }
    public required string BaseUrl { get; set; }
}
