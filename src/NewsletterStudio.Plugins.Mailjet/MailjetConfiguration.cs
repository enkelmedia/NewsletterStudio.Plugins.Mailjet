using System.Collections.Generic;

namespace NewsletterStudio.Plugins.Mailjet
{
    public class MailjetConfiguration
    {
        private readonly Dictionary<string, object> _settings;

        public MailjetConfiguration(Dictionary<string,object> settings)
        {
            if (settings == null)
                settings = new Dictionary<string, object>();

            _settings = settings;
        }

        public string ApiKey
        {
            get
            {
                if (_settings.ContainsKey("mj_apiKey"))
                    return _settings["mj_apiKey"].ToString();

                return null;
            }
        }

        public string ApiSecret
        {
            get
            {
                if (_settings.ContainsKey("mj_apiSecret"))
                    return _settings["mj_apiSecret"].ToString();

                return null;
            }
        }

        public bool SandboxMode
        {
            get
            {
                if (_settings.ContainsKey("mj_sandboxMode"))
                    return _settings["mj_sandboxMode"].ToString().Equals("true");

                return false;
            }
        }
        
    }
}