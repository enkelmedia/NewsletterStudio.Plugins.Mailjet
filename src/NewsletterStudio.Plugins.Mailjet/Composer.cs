using Microsoft.Extensions.DependencyInjection;
using NewsletterStudio.Core.Composing;
using NewsletterStudio.Plugins.Mailjet.Backoffice.Api;
using Umbraco.Cms.Api.Common.OpenApi;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;

namespace NewsletterStudio.Plugins.Mailjet;

public class Composer : IComposer
{
    public void Compose(IUmbracoBuilder builder)
    {
        builder.NewsletterStudio().EmailServiceProviders.Append<MailjetEmailServiceProvider>();

#if DEBUG
        //// SWAGGER - Only use in debug build to avoid exposing in production messing up things in the core.
        builder.Services.ConfigureOptions<ConfigureNewsletterStudioPluginApiSwaggerGenOptions>();
        builder.Services.AddSingleton<ISchemaIdHandler, NewsletterStudioPluginSchemaIdHandler>();
        builder.Services.AddSingleton<IOperationIdHandler, NewsletterStudioPluginOperationIdHandler>();
#endif
    }
}
