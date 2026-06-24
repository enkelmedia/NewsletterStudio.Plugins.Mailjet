using System.Text;
using NewsletterStudio.Core.Services;
using NewsletterStudio.Core.Public;
using NewsletterStudio.Plugins.Mailjet.Dtos;
using NewsletterStudio.Plugins.Mailjet.Webhook.Models;
using Umbraco.Extensions;
using Microsoft.AspNetCore.Mvc;
using NewsletterStudio.Core;

namespace NewsletterStudio.Plugins.Mailjet.Webhook;

public class MailjetWebhookController : Controller
{
    private readonly IBounceOperationsService _bounceOperationsService;
    private readonly INewsletterStudioService _newsletterStudioService;

    public MailjetWebhookController(
        IBounceOperationsService bounceOperationsService,
        INewsletterStudioService newsletterStudioService
    ) 
    {
        _bounceOperationsService = bounceOperationsService;
        _newsletterStudioService = newsletterStudioService;
    }

    [HttpPost]
    [Route(NewsletterStudioConstants.Paths.Routes.ControllersRootRoute + "mailjet/webhook")]
    public async Task<IActionResult> Webhook([FromBody]List<MailJetWebhookEvent> events, [FromQuery] string secret)
    {
        var result = await Do_HandleAsync(secret, events).ConfigureAwait(false);

        if(result.Success)
            return Content("Done");

        Response.StatusCode = 400;
        return Content(result.Message);
    }

    private async Task<DoHandleResponse> Do_HandleAsync(string secret, List<MailJetWebhookEvent> events)
    {
        if (string.IsNullOrEmpty(secret))
            return new DoHandleResponse(false, "Secret was empty");

        if(!Guid.TryParse(secret, out Guid workspaceKey))
            return new DoHandleResponse(false, "Invalid key-format");

        var validKeys = await GetValidWorkspaceKeysAsync().ConfigureAwait(false);

        if (!validKeys.Contains(workspaceKey))
            return new DoHandleResponse(false, "Invalid key");

        foreach (var mjEvent in events)
        {
            string externalId = mjEvent.MessageId.ToString();
            string errorMessage = await HandleAndExtractErrorMessageAsync(mjEvent).ConfigureAwait(false);

            await _bounceOperationsService.SetTrackingItemErrorAsync(externalId, errorMessage).ConfigureAwait(false);
        }

        return new DoHandleResponse()
        {
            Success = true
        };
    }

    private async Task<List<Guid>> GetValidWorkspaceKeysAsync()
    {
        var workspacesAndLists = await _newsletterStudioService.GetMailingListsForAllWorkspacesAsync().ConfigureAwait(false);
        return workspacesAndLists.Select(x => x.UniqueKey).ToList();
    }

    private async Task<string> HandleAndExtractErrorMessageAsync(MailJetWebhookEvent mjEvent)
    {
        StringBuilder sb = new StringBuilder();

        if (mjEvent.Event.Equals("spam"))
        {
            sb.Append($"Message reported as spam. Loop program reported: {mjEvent.Source}.");
        }
        else
        {
            // mjEvent.Event = "blocked" or "bounce"
            sb.Append($"Issue: {mjEvent.Event}.");
            sb.Append($"Error: {mjEvent.Error ?? " n/a"}. ");
            sb.Append($"Comment: {mjEvent.Comment ?? "n/a"}. ");

            if (mjEvent.HardBounce.HasValue && mjEvent.HardBounce.Value)
            {
                await SetRecipientAsPermanentErrorAsync(mjEvent,sb.ToString()).ConfigureAwait(false);
            }
        }

        return sb.ToString();
    }

    public Task SetRecipientAsPermanentErrorAsync(MailJetWebhookEvent mjEvent,string errorMessage)
    {
        //NOTE: There are currently no way to pass a message to explain why a recipient
        //      was set as permanent error so we can't pass anything.
        return _bounceOperationsService.SetRecipientPermanentErrorAsync(mjEvent.MessageId.ToString());
    }
        
}
