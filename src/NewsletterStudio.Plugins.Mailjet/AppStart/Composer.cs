using NewsletterStudio.Plugins.Mailjet;
using NewsletterStudio.Web.Composing;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;

namespace NewsletterStudio.Plugins.Mailjet.AppStart
{
    public class Composer : IComposer
    {
        public void Compose(IUmbracoBuilder builder)
        {
            builder.NewsletterStudio().EmailServiceProviders.Append<MailjetEmailServiceProvider>();
        }
    }
}