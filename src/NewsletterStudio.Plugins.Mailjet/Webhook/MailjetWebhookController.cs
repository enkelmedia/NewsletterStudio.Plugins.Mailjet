using NewsletterStudio.Core.Services;
using NewsletterStudio.Core.Public;
using NewsletterStudio.Plugins.Mailjet.Dtos;
using NewsletterStudio.Plugins.Mailjet.Webhook.Models;
using Newtonsoft.Json;
using Umbraco.Extensions;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Logging;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Web.Website.Controllers;

namespace NewsletterStudio.Plugins.Mailjet.Webhook
{
    public class MailjetWebhookController : SurfaceController
    {
        private readonly IBounceOperationsService _bounceOperationsService;
        private readonly INewsletterStudioService _newsletterStudioService;
        private readonly AppCaches _appCaches;


        public MailjetWebhookController(
            IUmbracoContextAccessor umbracoContextAccessor,
            IUmbracoDatabaseFactory databaseFactory,
            ServiceContext services,
            AppCaches appCaches,
            IProfilingLogger profilingLogger,
            IPublishedUrlProvider publishedUrlProvider,
            IBounceOperationsService bounceOperationsService,
            INewsletterStudioService newsletterStudioService
        ) : base(umbracoContextAccessor, databaseFactory, services, appCaches, profilingLogger, publishedUrlProvider)
        {
            _bounceOperationsService = bounceOperationsService;
            _newsletterStudioService = newsletterStudioService;
        }

        /// umbraco/surface/MailjetWebhook/Handle

        [HttpPost]
        public IActionResult Handle(string secret)
        {
            string body = new StreamReader(Request.Body).ReadToEnd();

            var res = Do_Handle(secret,body); 

            if(res.Success)
                return Content("Done");


            Response.StatusCode = 400;
            return Content(res.Message);
        }
        private DoHandleResponse Do_Handle(string secret, string body)
        {
            if (string.IsNullOrEmpty(secret))
                return new DoHandleResponse(false, "Secret was empty");

            if(!Guid.TryParse(secret, out Guid workspaceKey))
                return new DoHandleResponse(false, "Invalid key-format");

            var validKeys = GetValidWorkspaceKeys();

            if (!validKeys.Contains(workspaceKey))
                return new DoHandleResponse(false, "Invalid key");

            var mjEvents = JsonConvert.DeserializeObject<List<MailJetWebhookEvent>>(body);

            foreach (var mjEvent in mjEvents)
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

            },TimeSpan.FromHours(1));

        }
        
    }
}