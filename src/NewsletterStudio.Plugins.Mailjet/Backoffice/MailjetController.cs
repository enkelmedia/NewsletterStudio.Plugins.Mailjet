using Mailjet.Client;
using NewsletterStudio.Plugins.Mailjet.Backoffice.Models;
using NewsletterStudio.Plugins.Mailjet.Dtos;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Extensions;
using Microsoft.AspNetCore.Http;
using Umbraco.Cms.Api.Common.Attributes;
using NewsletterStudio.Plugins.Mailjet.Backoffice.Api;
using Umbraco.Cms.Web.Common.Routing;
using Umbraco.Cms.Api.Management.Controllers;


namespace NewsletterStudio.Plugins.Mailjet.Backoffice;

internal static class EndpointConfiguration
{
    public const string RouteSegment = "newsletter-studio-mailjet";
    public const string GroupName = "Mailjet";
}

/// <summary>
/// The controller
/// </summary>
[ApiExplorerSettings(GroupName = EndpointConfiguration.GroupName)]
[BackOfficeRoute($"{Umbraco.Cms.Core.Constants.Web.ManagementApiPath}{EndpointConfiguration.RouteSegment}")]
[MapToApi(NewsletterStudioPluginApiConfiguration.ApiName)]
public class MailjetController : ManagementApiControllerBase
{
    /// <summary>
    /// Get configuration
    /// </summary>
    /// <param name="req"></param>
    /// <returns></returns>
    [HttpPost("get-configuration",Name="Sven")]
    [ProducesResponseType(typeof(CheckWebhookConfigurationResponse), StatusCodes.Status200OK)]
    public IActionResult GetConfiguration(CheckWebhookConfigurationRequest req)
    {
        var mailjetConfiguration = new MailjetConfiguration(req.Settings);

        var res = new CheckWebhookConfigurationResponse()
        {
            WebhookUrl = GenerateWebhookUrl(req),
            IsBaseUrlLocalhost = this.IsUrlLocalHost(req.BaseUrl)
        };

        return Ok(res);
    }


    [HttpPost("configure-now", Name = "Kalle")]
    [ProducesResponseType(typeof(CheckWebhookConfigurationResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> ConfigureNow(CheckWebhookConfigurationRequest req)
    {
        var configuration = new MailjetConfiguration(req.Settings);

        string webhookUrl = GenerateWebhookUrl(req);

        MailjetClient client = new MailjetClient(configuration.ApiKey, configuration.ApiSecret)
        {
            Version = ApiVersion.V3
        };

        MailjetRequest request = new MailjetRequest
        {
            Resource = global::Mailjet.Client.Resources.Eventcallbackurl.Resource
        };

        MailjetResponse response = await client.GetAsync(request).ConfigureAwait(false);

        if (response.IsSuccessStatusCode)
        {
            var data = response.GetData();

            var allConfiguredWebhooks = data.ToObject<List<MailjetWebhookDto>>()!;

            // Webhook types we want to create with Mailjet
            List<string> eventTypesToCreate = new List<string>() {"bounce", "spam", "blocked"};

            // Iterate any existing webhooks that matches "our pattern",
            // we'll remove them later on after creating the new once.
            if (allConfiguredWebhooks.Any())
            {
                var nsWebhooks = allConfiguredWebhooks.Where(x => x.Url.Contains("webhook?secret=")).ToList();

                if (nsWebhooks.Any())
                {
                    foreach (var nsWebhook in nsWebhooks)
                    {
                        // If the existing webhook matches our webhookUrl we don't have to re-create.
                        if (nsWebhook.Url != webhookUrl)
                        {
                            // Delete
                            MailjetRequest delRequest = new MailjetRequest
                            {
                                Resource = global::Mailjet.Client.Resources.Eventcallbackurl.Resource,
                                ResourceId = ResourceId.Numeric(nsWebhook.Id)
                            };

                            MailjetResponse delResponse = await client.DeleteAsync(delRequest).ConfigureAwait(false);

                            var test = delResponse;

                        }
                        else
                        {
                            eventTypesToCreate.Remove(nsWebhook.EventType);
                        }
                            
                    }
                }

            }


            if (eventTypesToCreate.Any())
            {
                // create
                foreach (var eventType in eventTypesToCreate)
                {
                    MailjetRequest createRequest = new MailjetRequest
                    {
                        Resource = global::Mailjet.Client.Resources.Eventcallbackurl.Resource
                    };
                    createRequest.Property(global::Mailjet.Client.Resources.Eventcallbackurl.EventType, eventType);
                    createRequest.Property(global::Mailjet.Client.Resources.Eventcallbackurl.Url, webhookUrl);
                    createRequest.Property(global::Mailjet.Client.Resources.Eventcallbackurl.IsBackup, false);
                    createRequest.Property(global::Mailjet.Client.Resources.Eventcallbackurl.Version, 2);
                    createRequest.Property(global::Mailjet.Client.Resources.Eventcallbackurl.Status, "alive");

                    MailjetResponse createResponse = await client.PostAsync(createRequest).ConfigureAwait(false);

                    var createWasSuccess = createResponse.IsSuccessStatusCode;
                    var createResponseData = createResponse.GetData();
                    var createResponseDataAgain = createResponseData;

                }

            }

        }
        else
        {
            //TODO: Logging
            return BadRequest($"Could not connect to the MailJet-server, response was: {response.GetErrorMessage()}.");
        }

        var res = new CheckWebhookConfigurationResponse()
        {
            WebhookUrl =GenerateWebhookUrl(req)
        };

        return Ok(res);
    }


    private string GenerateWebhookUrl(CheckWebhookConfigurationRequest req)
    {
        string baseUrl = req.BaseUrl.EnsureEndsWith("/");

        return $"{baseUrl}__ns/mailjet/webhook?secret={req.WorkspaceKey.ToString("N")}";

    }

    private bool IsUrlLocalHost(string url)
    {
        try
        {
            var uri = new Uri(url);
            return uri.Host.Equals("localhost", StringComparison.InvariantCultureIgnoreCase);
        }
        catch (Exception)
        {
            return false;
        }
    }

}
