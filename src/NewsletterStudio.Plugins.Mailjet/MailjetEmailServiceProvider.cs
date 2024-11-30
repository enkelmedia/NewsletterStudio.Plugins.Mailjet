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
            errors.Add(new ValidationError(Constants.Settings.ApiKeyPropertyName, "ns_validationRequired")); //TODO: Use ValidationErrors.GenericRequired

        if (string.IsNullOrEmpty(configuration.ApiSecret))
            errors.Add(new ValidationError(Constants.Settings.SecretPropertyName, "ns_validationRequired")); //TODO: Use ValidationErrors.GenericRequired

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

        MailjetResponse response = await client.PostAsync(request).ConfigureAwait(false);

        if (response.IsSuccessStatusCode)
        {
            var data = response.GetData();

            for (int i = 0; i < batch.Count; i++)
            {
                var mailJetResult = data[i].ToObject<MailJetMessageResponse>();

                if (mailJetResult == null)
                    throw new ArgumentException("No MailJet-result for message");

                batch[i].Successful = mailJetResult.Successful;

                // Storing the "MessageId" as "ExternalId" for bounce reporting
                batch[i].ExternalId = mailJetResult.To.First().MessageId.ToString();

                if (mailJetResult.Successful == false)
                {
                    batch[i].ErrorMessage = string.Join(", ", mailJetResult.Errors.Select(x => x.ErrorMessage));
                }

            }
        }
        else
        {
            var error = response.GetData()[0].ToObject<MailJetMessageError>();

            for (int i = 0; i < batch.Count; i++)
            {
                batch[i].Successful = false;
                batch[i].ErrorMessage = $"{error.ErrorMessage} ErrorCode: {error.ErrorCode}, ErrorRelatedTo: {string.Join(",", error.ErrorRelatedTo)}.";
            }
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



    private JToken MailjetMessageFromBatchJOb(SendEmailJob batchItem)
    {

        var message = new JObject
        {
            {
                "From",
                new JObject {{"Email", batchItem.Message.From.Email}, {"Name", batchItem.Message.From.DisplayName}}
            },
            {"Subject", batchItem.Message.Subject},
            {"HTMLPart", batchItem.Message.HtmlBody}
        };
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
