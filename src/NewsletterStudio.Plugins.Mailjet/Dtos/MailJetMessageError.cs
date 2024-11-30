using System;

namespace NewsletterStudio.Plugins.Mailjet.Dtos;

/// <summary>
/// Generic error details for Mailjet
/// </summary>
public class MailJetMessageError
{
    public Guid? ErrorIdentifier { get; set; }
    public string? ErrorCode { get; set; }
    public int? StatusCode {get; set; }
    public string? ErrorMessage { get; set; }
        
    public string[]? ErrorRelatedTo { get;set; }
       
}
