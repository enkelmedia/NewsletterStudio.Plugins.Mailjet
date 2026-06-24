using NewsletterStudio.Core.Composing;
using NewsletterStudio.Plugins.Mailjet.Backoffice.Api;
using Umbraco.Cms.Api.Common.OpenApi;
using Umbraco.Cms.Api.Management.OpenApi;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Extensions;

namespace NewsletterStudio.Plugins.Mailjet;

public class Composer : IComposer
{
    public void Compose(IUmbracoBuilder builder)
    {
        builder.NewsletterStudio().EmailServiceProviders.Append<MailjetEmailServiceProvider>();

        // OPEN API - Only use in debug build to avoid exposing in production messing up things in the core.
        #if DEBUG
        builder.AddBackOfficeOpenApiDocument(
            NewsletterStudioPluginApiConfiguration.ApiName,
            document => document
                .WithTitle(NewsletterStudioPluginApiConfiguration.ApiTitle)
                .WithBackOfficeAuthentication()
                .WithJsonOptions(Umbraco.Cms.Core.Constants.JsonOptionsNames.BackOffice)
                .ConfigureOpenApiOptions(options => options.AddOperationTransformer((operation, context, _) =>
                {
                    if (context.Description.ActionDescriptor.RouteValues.TryGetValue("action", out var actionName)
                        && !string.IsNullOrWhiteSpace(actionName))
                    {
                        operation.OperationId = actionName.ToFirstLower();
                    }

                    return Task.CompletedTask;
                })));

        #endif
    }
}
