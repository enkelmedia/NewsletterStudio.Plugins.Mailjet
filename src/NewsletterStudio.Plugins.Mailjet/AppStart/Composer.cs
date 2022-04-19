using NewsletterStudio.Plugins.Mailjet;
using NewsletterStudio.Web.Composing;

#if NETFRAMEWORK
using Umbraco.Core.Composing;
#else
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
#endif

namespace NewsletterStudio.Plugins.Mailjet.AppStart
{
#if NETFRAMEWORK

    public class Composer : IUserComposer
    {
        public void Compose(Composition composition)
        {
            composition.NewsletterStudio().EmailServiceProviders.Append<MailjetEmailServiceProvider>();
        }
    }
#else
    public class Composer : IComposer
    {
        public void Compose(IUmbracoBuilder builder)
        {
            builder.NewsletterStudio().EmailServiceProviders.Append<MailjetEmailServiceProvider>();
        }
    }

#endif


}