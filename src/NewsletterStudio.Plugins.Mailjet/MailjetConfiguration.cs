namespace NewsletterStudio.Plugins.Mailjet;

public class MailjetConfiguration
{
    private readonly Dictionary<string, object> _settings;

    public MailjetConfiguration(Dictionary<string,object>? settings)
    {
        if (settings == null)
            settings = new Dictionary<string, object>();

        _settings = settings;
    }

    public string? ApiKey
    {
        get
        {
            if (_settings.ContainsKey(Constants.Settings.ApiKeyPropertyName))
                return _settings[Constants.Settings.ApiKeyPropertyName].ToString();

            return null;
        }
    }

    public string? ApiSecret
    {
        get
        {
            if (_settings.ContainsKey(Constants.Settings.SecretPropertyName))
                return _settings[Constants.Settings.SecretPropertyName].ToString();

            return null;
        }
    }

    public bool SandboxMode
    {
        get
        {
            if (_settings.ContainsKey(Constants.Settings.SandboxModePropertyName))
                return _settings[Constants.Settings.SandboxModePropertyName].ToString()?.Equals("true",StringComparison.InvariantCultureIgnoreCase) == true;

            return false;
        }
    }
        
}
