import type { UmbLocalizationDictionary } from '@umbraco-cms/backoffice/localization-api';

export default {
    ns : {
        mailjet_apiKey : 'API Key',
        mailjet_apiKeyDescription : 'Enter API-key for Mailjet',
        mailjet_apiSecret : 'API Secret',
        mailjet_apiSecretDescription : 'Enter secret for Mailjet API',
        mailjet_sandboxMode : 'Sandbox Mode',
        mailjet_sandboxModeDescription : 'Check to activate Mailjet Sandbox mode',

        mailjet_bounceManagementHeader : 'Setting up bounce management',
        mailjet_bounceManagementIntroduction : 'The plugin can help you to setup bounce management using <a href="https://dev.mailjet.com/email/guides/webhooks/" target="_blank">Mailjet Webhooks</a>. When activated, Mailjet will send webhooks to update your contact list based on any bounces.',
        mailjet_bounceManagementNotLocalhost : 'This only works when the configured Base Url for the workspace is a public accessible URL (eg. not localhost).'
    }
} as UmbLocalizationDictionary
