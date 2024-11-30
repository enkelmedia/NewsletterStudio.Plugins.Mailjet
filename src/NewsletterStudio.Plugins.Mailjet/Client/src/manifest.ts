import { ManifestEmailServiceProviderSettingsUi } from "@newsletterstudio/umbraco/extensibility";
import type { ManifestLocalization, UmbBackofficeExtensionRegistry } from "@umbraco-cms/backoffice/extension-registry";

const smtpMailjetUi : ManifestEmailServiceProviderSettingsUi = {
  type: "nsEmailServiceProviderSettingsUi",
  name: "Newsletter Studio Smtp Mailjet Provider Settings",
  alias: "Ns.EmailServiceProviderUi.SmtpMailjet",
  element: () => import('./ns-email-service-settings-mailjet.element.js'),
  meta: {
    alias : 'mailjet'
  }
};

const translationManifests : Array<ManifestLocalization> = [
	{
		type: "localization",
		alias: "UmbNs.Localize.EnUS",
		name: "English (United States)",
		meta: {
			"culture": "en-us"
		},
		js : ()=> import('./localization/en-us.js')
	}
]

export function registerManifest(registry : UmbBackofficeExtensionRegistry) {

    registry.registerMany([
		smtpMailjetUi,
    ...translationManifests
	]);
}
