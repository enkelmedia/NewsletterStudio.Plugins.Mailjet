using System;

namespace NewsletterStudio.Plugins.Mailjet.Dtos
{
    /// <summary>
    /// Represents a individual email sent by Mailjet to a recipient (To, Cc or Bcc)
    /// </summary>
    public class MailJetMessageRecipientMessageResult
    {
        public string Email { get;set; }
        public string MessageUuid { get;set; }
        public Int64 MessageId { get;set; }
        public string MessageHref { get; set; }
    }
}