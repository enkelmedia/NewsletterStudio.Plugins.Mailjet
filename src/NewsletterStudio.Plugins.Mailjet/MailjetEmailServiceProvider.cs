using System.Text;
using Mailjet.Client;
using Microsoft.Extensions.Logging;
using NewsletterStudio.Core.Models;
using NewsletterStudio.Core.Models.System;
using NewsletterStudio.Core.Notifications;
using NewsletterStudio.Core.Sending;
using NewsletterStudio.Core.Sending.Providers;
using NewsletterStudio.Plugins.Mailjet.Dtos;
using Newtonsoft.Json.Linq;
using Umbraco.Cms.Core.Events;

namespace NewsletterStudio.Plugins.Mailjet;

public class MailjetEmailServiceProvider : IEmailServiceProvider
{
    private readonly ILogger<MailjetEmailServiceProvider> _logger;
    private readonly IEventAggregator _eventAggregator;

    public const string ProviderAlias = "mailjet";

    public string Alias => ProviderAlias;
    public string DisplayName => "Mailjet";
    public Dictionary<string, object> Settings { get; set; }

    public MailjetEmailServiceProvider(
        ILogger<MailjetEmailServiceProvider> logger,
        IEventAggregator eventAggregator
    )
    {
        _logger = logger;
        _eventAggregator = eventAggregator;
    }

    public SendOutConfiguration GetSendOutConfiguration()
    {
        return new SendOutConfiguration() {MaxItemsPerBatch = 25, SendBatchSize = 25};
    }

    public ErrorCollection ValidateSettings(Dictionary<string, object> settings)
    {
        var configuration = new MailjetConfiguration(settings);

        var errors = new ErrorCollection();

        if (string.IsNullOrEmpty(configuration.ApiKey))
            errors.Add(new ValidationError(Constants.Settings.ApiKeyPropertyName, ValidationErrors.GenericRequired));

        if (string.IsNullOrEmpty(configuration.ApiSecret))
            errors.Add(new ValidationError(Constants.Settings.SecretPropertyName, ValidationErrors.GenericRequired));

        return errors;

    }

    public async Task SendAsync(List<SendEmailJob> batch)
    {
        var configuration = new MailjetConfiguration(Settings);

        if (batch.Count > 50)
            throw new ArgumentException("Can't send more than 50 emails per batch with Mailjet.");

        MailjetRequest request = new MailjetRequest {Resource = global::Mailjet.Client.Resources.Send.Resource};

        var arr = new JArray();
        foreach (var message in batch)
        {
            var messageObject = MailjetMessageFromBatchJOb(message);
            arr.Add(messageObject);
        }

        request.Property(global::Mailjet.Client.Resources.Send.Messages, arr);

        if (configuration.SandboxMode)
        {
            request.Property("SandboxMode", true);
        }

        // This will disable open and click-tracking from Mailjet
        request.Property("Globals", new JObject {{"TrackOpens", "disabled"}, {"TrackClicks", "disabled"}}
        );

        MailjetClient client = new MailjetClient(configuration.ApiKey, configuration.ApiSecret)
        {
            Version = ApiVersion.V3_1
        };

        // Fires the EmailSendingNotification to allow package consumers to make adjustments to the
        // request just before the message(s) are passed over to MailJet.
        await _eventAggregator.PublishAsync(new EmailSendingNotification(request)).ConfigureAwait(false);

        // The response will contain an array in the same order as we sent them to the API.
        // EACH item in the array have a "Status" ("success" or "error")
        // Good to know: mailjetResponseData.IsSuccessStatusCode will be true if ALL messages
        // was sent successfully, if any message failed, it will be false.
        // Details: https://dev.mailjet.com/email/guides/webhooks/#event-types

        MailjetResponse? response = null;

        try
        {
            response = await client.PostAsync(request).ConfigureAwait(false);
        }
        catch (Exception e)
        {
            SetBatchAsFailed(batch,e);
            return;
        }

        try
        {
            var mailjetResponseData = response.GetData();

            for (int i = 0; i < batch.Count; i++)
            {
                var mailjetResult = mailjetResponseData[i].ToObject<MailJetMessageResponse>();

                if (mailjetResult == null)
                    throw new ArgumentException("No MailJet-result for message");

                batch[i].Successful = mailjetResult.Successful;

                if (mailjetResult.Successful)
                {
                    batch[i].ExternalId = mailjetResult.To.First().MessageId.ToString();
                }
                else
                {
                    batch[i].ErrorMessage = this.ErrorsAsString(mailjetResult.Errors);
                }
            }

        }
        catch (Exception e)
        {
            SetBatchAsFailed(batch,e,response);
        }

    }

    public async Task<CommandResult> SendAsync(EmailMessage message)
    {

        var fakeBulk = new List<SendEmailJob>();
        fakeBulk.Add(new SendEmailJob() {Message = message});

        await SendAsync(fakeBulk);

        var res = fakeBulk.First();

        if (res.Successful)
            return CommandResult.Successful();

        return CommandResult.Error(new ValidationError("", res.ErrorMessage));

    }

    private string ErrorsAsString(List<MailJetMessageError> errors)
    {
        StringBuilder sb = new StringBuilder();

        foreach (var error in errors)
        {
            sb.Append($"{error.ErrorMessage} ErrorCode: {error.ErrorCode}, ErrorRelatedTo: {string.Join(",", error.ErrorRelatedTo ?? [])}. Identifier: {error.ErrorIdentifier}");
        }

        return sb.ToString();
    }

    private void SetBatchAsFailed(List<SendEmailJob> batch,Exception e, MailjetResponse? mailjetResponse = null)
    {
        _logger.LogError(e,"NewsletterStudio: Critical error in Mailjet-batch. All items in batch was reported as unsuccessful. Mailjet response: {MailjetResponse}", mailjetResponse);

        foreach (var item in batch)
        {
            item.Successful = false;
            item.ErrorMessage = $"Critical error/exception. {e.Message}. See Trace Log for more details.";
        }
    }

    private JToken MailjetMessageFromBatchJOb(SendEmailJob batchItem)
    {

        var message = new JObject
        {
            {
                "From", new JObject {{"Email", batchItem.Message.From.Email}, {"Name", batchItem.Message.From.DisplayName}}
            },
            {"Subject", batchItem.Message.Subject},
            {"HTMLPart", batchItem.Message.HtmlBody}
        };

        // Using WorkspaceKey as CustomID so that webhook-handler can know if needed.
        message.Add("CustomID", batchItem.WorkspaceKey.ToString());

        message.Add("To", RecipientsToJArray(batchItem.Message.To));

        if (batchItem.Message.Cc.Any())
        {
            message.Add("Cc", RecipientsToJArray(batchItem.Message.Cc));
        }

        if (batchItem.Message.Bcc.Any())
        {
            message.Add("Bcc", RecipientsToJArray(batchItem.Message.Bcc));
        }

        return message;

    }

    private JArray RecipientsToJArray(EmailAddressCollection collection)
    {
        var arr = new JArray();

        foreach (var email in collection)
        {
            arr.Add(new JObject {{"Email", email.Email}, {"Name", email.DisplayName}});
        }

        return arr;
    }

}
