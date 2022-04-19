using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Mailjet.Client;
using NewsletterStudio.Core;
using NewsletterStudio.Plugins.Mailjet.Backoffice.Models;
using NewsletterStudio.Plugins.Mailjet.Dtos;


#if NETFRAMEWORK
using System.Web.Http;
using Umbraco.Core;
using Umbraco.Web.Editors;
using Umbraco.Web.Mvc;
using Umbraco.Web.WebApi;
#else
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Web.BackOffice.Controllers; 
using Umbraco.Cms.Web.Common.Attributes;
using Umbraco.Cms.Web.BackOffice.Filters;
using Umbraco.Extensions;
#endif

namespace NewsletterStudio.Plugins.Mailjet.Backoffice 
{
    [PluginController("NewsletterStudioMailjet")]
    [JsonCamelCaseFormatter]
    public class MailjetController : UmbracoAuthorizedJsonController 
    {

#if NETFRAMEWORK
        [HttpPost]
        public IHttpActionResult GetConfiguration(CheckWebhookConfigurationRequest req)
#else
        [HttpPost] 
        public IActionResult GetConfiguration(CheckWebhookConfigurationRequest req)
#endif
        {
            var mailjetConfiguration = new MailjetConfiguration(req.Settings);

            var res = new CheckWebhookConfigurationResponse();
            res.WebhookUrl = GenerateWebhookUrl(req);
            res.IsBaseUrlLocalhost = this.IsUrlLocalHost(req.BaseUrl);

            return Ok(res);
        }

#if NETFRAMEWORK
        [HttpPost]
        public IHttpActionResult ConfigureNow(CheckWebhookConfigurationRequest req)
#else
        [HttpPost]
        public IActionResult ConfigureNow(CheckWebhookConfigurationRequest req)
#endif
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

            MailjetResponse response = Task.Run(() => client.GetAsync(request)).Result;

            if (response.IsSuccessStatusCode)
            {
                var data = response.GetData();

                var allConfiguredWebhooks = data.ToObject<List<MailjetWebhookDto>>();
                
                var testData = allConfiguredWebhooks.Count;

                List<string> eventTypesToCreate = new List<string>() {"bounce", "spam", "blocked"};

                if (allConfiguredWebhooks.Any())
                {
                    var nsWebhooks = allConfiguredWebhooks.Where(x => x.Url.Contains("handle?secret=")).ToList();

                    if (nsWebhooks.Any())
                    {
                        foreach (var nsWebhook in nsWebhooks)
                        {
                            if (nsWebhook.Url != webhookUrl)
                            {
                                // Delete
                                MailjetRequest delRequest = new MailjetRequest
                                {
                                    Resource = global::Mailjet.Client.Resources.Eventcallbackurl.Resource,
                                    ResourceId = ResourceId.Numeric(nsWebhook.Id)
                                };

                                MailjetResponse delResponse = Task.Run(() => client.DeleteAsync(delRequest)).Result;

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

                        MailjetResponse createResponse = Task.Run(() => client.PostAsync(createRequest)).Result;

                        var test = createResponse.IsSuccessStatusCode;
                        var testResData = createResponse.GetData();
                        var restRestDataInfo = testResData;

                    }

                }


                MailjetResponse deleteReq = Task.Run(() => client.DeleteAsync(request)).Result;


            }

            var res = new CheckWebhookConfigurationResponse();
            res.WebhookUrl = GenerateWebhookUrl(req);

            return Ok(res);
        }


        private string GenerateWebhookUrl(CheckWebhookConfigurationRequest req)
        {
            string baseUrl = req.BaseUrl.EnsureEndsWith("/");

            return $"{baseUrl}umbraco/surface/mailjetwebhook/handle?secret={req.WorkspaceKey.Replace("-", "")}";
        }

        private bool IsUrlLocalHost(string url)
        {
            try
            {
                var uri = new Uri(url);
                return uri.Host.Equals("localhost", StringComparison.InvariantCultureIgnoreCase);
            }
            catch (Exception e)
            {
                return false;
            }
        }

    }
    
}