using System.Text;
using NewsletterStudio.Core.Services;
using NewsletterStudio.Core.Public;
using NewsletterStudio.Plugins.Mailjet.Dtos;
using NewsletterStudio.Plugins.Mailjet.Webhook.Models;
using Umbraco.Extensions;
using Umbraco.Cms.Core.Cache;
using Microsoft.AspNetCore.Mvc;
using NewsletterStudio.Core;
using NewsletterStudio.Core.Frontend.Common.FrontendModels;
using Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_13_5_0;

namespace NewsletterStudio.Plugins.Mailjet.Webhook;

public class MailjetWebhookController : Controller
{
    private readonly IBounceOperationsService _bounceOperationsService;
    private readonly INewsletterStudioService _newsletterStudioService;
    private readonly AppCaches _appCaches;

    public MailjetWebhookController(
        AppCaches appCaches,
        IBounceOperationsService bounceOperationsService,
        INewsletterStudioService newsletterStudioService
    ) 
    {
        _bounceOperationsService = bounceOperationsService;
        _newsletterStudioService = newsletterStudioService;
        _appCaches = appCaches;
    }

    [HttpPost]
    [Route(NewsletterStudioConstants.Paths.Routes.ControllersRootRoute + "mailjet/webhook")]
    public IActionResult Webhook([FromBody]List<MailJetWebhookEvent> events, [FromQuery] string secret)
    {
        var result = Do_Handle(secret, events); 

        if(result.Success)
            return Content("Done");

        Response.StatusCode = 400;
        return Content(result.Message);
    }

    private DoHandleResponse Do_Handle(string secret, List<MailJetWebhookEvent> events)
    {
        if (string.IsNullOrEmpty(secret))
            return new DoHandleResponse(false, "Secret was empty");

        if(!Guid.TryParse(secret, out Guid workspaceKey))
            return new DoHandleResponse(false, "Invalid key-format");

        var validKeys = GetValidWorkspaceKeys();

        if (!validKeys.Contains(workspaceKey))
            return new DoHandleResponse(false, "Invalid key");

        foreach (var mjEvent in events)
        {
            string externalId = mjEvent.MessageId.ToString();
            string errorMessage = HandleAndExtractErrorMessage(mjEvent);

            _bounceOperationsService.SetTrackingItemError(externalId, errorMessage);

            //TODO: Consider if it makes sense to ues _bounceOperationsService.SetRecipientPermanentError() for any errors.
        }

        return new DoHandleResponse()
        {
            Success = true
        };
    }

    private List<Guid> GetValidWorkspaceKeys()
    {
        return _appCaches.RuntimeCache.GetCacheItem<List<Guid>>("ns_mj_workspaces", () =>
        {
            var workspacesAndLists = _newsletterStudioService.GetMailingListsForAllWorkspaces();
            return workspacesAndLists.Select(x => x.UniqueKey).ToList();

        },TimeSpan.FromMinutes(2))!;
    }

    private string HandleAndExtractErrorMessage(MailJetWebhookEvent mjEvent)
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
                SetRecipientAsPermanentError(mjEvent);
            }
        }

        

        

        return sb.ToString();
    }

    public void SetRecipientAsPermanentError(MailJetWebhookEvent mjEvent)
    {

        bool hasWorkspaceKey = Guid.TryParse(mjEvent.CustomId, out Guid workspaceKey);
        if (hasWorkspaceKey)
        {
            _bounceOperationsService.SetRecipientPermanentError(mjEvent.Email,workspaceKey);
        }
        else
        {
            //TODO: When #3960 has been fixed we should use message id OR fallback to setting error for all recipients
            //      with the same email in all workspaces.
        }

    }
        
}
