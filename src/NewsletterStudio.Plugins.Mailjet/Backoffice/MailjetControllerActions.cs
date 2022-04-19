using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NewsletterStudio.Plugins.Mailjet.Backoffice.Models;

namespace NewsletterStudio.Plugins.Mailjet.Backoffice
{
    internal class MailjetControllerActions
    {

        public CheckWebhookConfigurationResponse CheckConfiguration()
        {
            return new CheckWebhookConfigurationResponse();
        }


    }

    

}
