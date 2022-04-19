using Newtonsoft.Json;

namespace NewsletterStudio.Plugins.Mailjet.Dtos
{
    internal class MailjetWebhookDto
    {
        /// <summary>
        /// Id for the webhook
        /// </summary>
        [JsonProperty("ID")]
        public long Id { get; set; }

        /// <summary>
        /// Holds the type of event for the webhook. Eg: bounce, spam, blocked
        /// </summary>
        [JsonProperty("EventType")]
        public string EventType { get; set; }

        /// <summary>
        /// Holds the status of the webhook. Eg: alive
        /// </summary>
        [JsonProperty("Status")]
        public string Status { get; set; }

        /// <summary>
        /// Holds the URL for the webhook
        /// </summary>
        [JsonProperty("Url")]
        public string Url { get; set; }

        /// <summary>
        /// 1 = Not grouped, 2 = grouped
        /// </summary>
        [JsonProperty("Version")]
        public int Version { get; set; }

    }
}