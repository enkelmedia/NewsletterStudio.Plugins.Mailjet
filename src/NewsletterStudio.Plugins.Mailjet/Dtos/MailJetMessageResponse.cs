using System.Collections.Generic;

namespace NewsletterStudio.Plugins.Mailjet.Dtos
{
    /// <summary>
    /// Response object from Mailjet API. Sending a batch will return one of these for each sent message.
    /// </summary>
    internal class MailJetMessageResponse
    {
        public bool Successful => this.Status == "success";

        /// <summary>
        /// Indicates the status, know values: success, error
        /// </summary>
        public string Status { get; set; }

        public List<MailJetMessageRecipientMessageResult> To { get; set; }
        public List<MailJetMessageRecipientMessageResult> Cc { get; set; }
        public List<MailJetMessageRecipientMessageResult> Bcc { get; set; }

        public List<MailJetMessageError> Errors { get;set; }
    }
}