namespace NewsletterStudio.Plugins.Mailjet.Dtos;

/// <summary>
/// Represents an individual email sent by Mailjet to a recipient (To, Cc or Bcc)
/// </summary>
public class MailJetMessageRecipientMessageResult
{
    public string Email { get; set; } = default!;
    public string MessageUuid { get;set; } = default!;
    public long MessageId { get;set; }
    public string MessageHref { get; set; } = default!;
}
