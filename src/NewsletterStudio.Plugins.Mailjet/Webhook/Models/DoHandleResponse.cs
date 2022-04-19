namespace NewsletterStudio.Plugins.Mailjet.Webhook.Models
{
    internal class DoHandleResponse
    {
        public DoHandleResponse()
        {
        }

        public DoHandleResponse(bool success, string message)
        {
            Success = success;
            Message = message;
        }

        public bool Success { get; set; }
        public string Message { get; set; }
    }
}