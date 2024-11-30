using System;
using Newtonsoft.Json;

namespace NewsletterStudio.Plugins.Mailjet.Dtos
{
    public class MailJetWebhookEvent
    {
        /// <summary>
        /// The event type. Valid values includes: bounce, blocked, spam
        /// </summary>
        public string Event { get; set; } = default!;

        /// <summary>
        /// Unix timestamp of event
        /// </summary>
        public int Time { get; set; }

        [JsonProperty("MessageID")]
        public long MessageId { get; set; }

        [JsonProperty("Message_GUID")]
        public Guid MessageGuid { get; set; }

        public string Email { get; set; }

        [JsonProperty("mj_contact_id")]
        public long MailjetContactId { get; set; }

        [JsonProperty("CustomID")]
        public string? CustomId { get; set; }

        /// <summary>
        /// Used for eventType: spam. Indicates which feedback loop program reported this complaint
        /// </summary>
        [JsonProperty("source")]
        public string? Source { get; set; }

        /// <summary>
        /// true if this bounce leads to the recipient being blocked
        /// </summary>
        [JsonProperty("blocked")]
        public bool? Blocked { get; set; }

        /// <summary>
        /// true if error was permanent
        /// </summary>
        [JsonProperty("hard_bounce")]
        public bool? HardBounce { get; set; }

        /// <summary>
        /// eventTypes: bounce & blocked. Holds a short description of any error.
        /// See error table: https://dev.mailjet.com/email/guides/webhooks/#possible-values-for-errors
        /// </summary>
        [JsonProperty("error")]
        public string? Error { get; set; }

        /// <summary>
        /// eventTypes: bounce & blocked. Holds a short description of any error.
        /// See error table: https://dev.mailjet.com/email/guides/webhooks/#possible-values-for-errors
        /// </summary>
        [JsonProperty("error_related_to")]
        public string? ErrorRelatedTo { get; set; }

        /// <summary>
        /// Used ie. for error details
        /// </summary>
        public string? Comment { get; set; }

    }

}
