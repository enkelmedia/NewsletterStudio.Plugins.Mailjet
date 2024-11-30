using NewsletterStudio.Core.Services;
using NewsletterStudio.Core.Public;
using NewsletterStudio.Plugins.Mailjet.Dtos;
using NewsletterStudio.Plugins.Mailjet.Webhook.Models;
using Umbraco.Extensions;
using Umbraco.Cms.Core.Cache;
using Microsoft.AspNetCore.Mvc;
using NewsletterStudio.Core;

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
        
        var res = Do_Handle(secret, events); 

        if(res.Success)
            return Content("Done");

        Response.StatusCode = 400;
        return Content(res.Message);
    }

    [HttpGet]
    [Route(NewsletterStudioConstants.Paths.Routes.ControllersRootRoute + "mailjet/foo")]
    public IActionResult Foo()
    {
        return Content("Bar");
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
            _bounceOperationsService.SetTrackingItemError(mjEvent.MessageId.ToString(), mjEvent.Error);
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
        
}
