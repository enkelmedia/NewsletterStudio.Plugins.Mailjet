import type { NsCheckboxElement } from '@newsletterstudio/umbraco/components';
import { tryExecute, tryExecuteAndNotify } from '@umbraco-cms/backoffice/resources';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import type { UUIButtonState } from '@umbraco-ui/uui';
import type { UmbNotificationContext } from "@umbraco-cms/backoffice/notification";
import { UMB_NOTIFICATION_CONTEXT } from "@umbraco-cms/backoffice/notification";
import { nsBindToValidation } from '@newsletterstudio/umbraco/forms';
import { css, html, customElement, when, state, unsafeHTML } from '@newsletterstudio/umbraco/lit';
import { NsEmailServiceProviderUiBase } from '@newsletterstudio/umbraco/extensibility';
import { WorkspaceManageOverviewResponseFrontendModel, WorkspaceManageValueFrontendModel, WorkspaceResource } from '@newsletterstudio/umbraco/backend';
import { GetConfigurationData, GetConfigurationResponse, MailjetResource } from './backend-api';
import {NS_ADMINISTRATION_WORKSPACE_CONTEXT, NsAdministrationWorkspaceContext} from '@newsletterstudio/umbraco/administration';
import { notifySuccess } from '@newsletterstudio/umbraco/core';
import { debounceTime, Observable } from '@umbraco-cms/backoffice/external/rxjs';

/**
* ns-email-service-settings-mailjet
* @element ns-email-service-settings-smtp
*/
@customElement('ns-email-service-settings-mailjet')
export class NsEmailServiceSettingsSmtpElement extends NsEmailServiceProviderUiBase<MailjetProviderSettings> {

  #notificationContext? : UmbNotificationContext;
  #workspaceContext? : NsAdministrationWorkspaceContext;

  @state()
  //workspaceEditModel? : WorkspaceManageValueFrontendModel;

  @state()
  configuration? : GetConfigurationResponse;

  @state()
  workspaceKey? : string;


  _baseUrl? : string;

  /**
   * A debounced observable of the workspace edit model so that we
   * can avoid server-fetch until user stops typing
   * */
  debouncedBaseUrlChange? : Observable<WorkspaceManageValueFrontendModel>;

  constructor() {
    super();

    this.consumeContext(UMB_NOTIFICATION_CONTEXT,(instance)=> {this.#notificationContext = instance});

    this.consumeContext(NS_ADMINISTRATION_WORKSPACE_CONTEXT,(instance) => {
      this.#workspaceContext = instance;
      console.log('ctx',instance);
      //debugger;

      this.debouncedBaseUrlChange = this.#workspaceContext.model.pipe(debounceTime(500));

      this.observe(this.#workspaceContext.model, (model) => {

        this._baseUrl = model.baseUrl;
        console.log('base url',this._baseUrl);
      });

      this.observe(this.debouncedBaseUrlChange, async ()=>{
        await this.#loadConfigurationFromServer();
        console.log('Value changed!!!!');
        // trigger reload of configuration from server.
      });

      var res = this.#workspaceContext.getWorkspaceKey();
      console.log('res workspace key',res);

      this.observe(this.#workspaceContext.workspaceKey,(workspaceKey) => {
        console.log('workspace key',workspaceKey);
        this.workspaceKey = workspaceKey;
        console.log('res workspace key', this.#workspaceContext!.getWorkspaceKey());
      });

    });

  }

  async connectedCallback() {
    super.connectedCallback();

    await this.#loadConfigurationFromServer();

  }

  async #loadConfigurationFromServer(){

    var configurationResult = await tryExecuteAndNotify(this,MailjetResource.getConfiguration({
      requestBody : this.#mapSettingsToModel()
    }));

    if(!configurationResult.error){
      this.configuration = configurationResult.data;
    }
  }

  #mapSettingsToModel() {

    return {
      settings : this.settings!,
      workspaceKey : this.workspaceKey ?? "",
      baseUrl : this._baseUrl ?? ""
    }

  }

  async #handleConfigureNow(){

    var configurationResult = await tryExecuteAndNotify(this,MailjetResource.configureNow({
      requestBody : this.#mapSettingsToModel()
    }));

    if(!configurationResult.error){
      notifySuccess(this,'Webhook created');
    }

  }

  renderSettings(settings : MailjetProviderSettings) {

    return html`
      <ns-validation-errors-debug></ns-validation-errors-debug>
      <ns-property
        label="ns_mailjet_apiKey"
        description="ns_mailjet_apiKeyDescription" required>
        <uui-form-layout-item>
          <uui-input type="text"
                      .value=${settings.mj_apiKey ?? ''}
                      name="host"
                      @change=${(e:Event)=>this.updateValueFromEvent('mj_apiKey',e)}
                      label=${this.localize.term('ns_mailjet_apiKey')}
                      ${nsBindToValidation(this,'$.mj_apiKey',settings.mj_apiKey)}
                      required></uui-input>
        </uui-form-layout-item>
      </ns-property>
      <ns-property
        label="ns_mailjet_apiSecret"
        description="ns_mailjet_apiSecretDescription" required>
        <uui-form-layout-item>
          <uui-input type="text"
                      .value=${settings.mj_apiSecret ?? ''}
                      name="host"
                      @change=${(e:Event)=>this.updateValueFromEvent('mj_apiSecret',e)}
                      label=${this.localize.term('ns_mailjet_apiSecret')}
                      ${nsBindToValidation(this,'$.mj_apiSecret',settings.mj_apiSecret)}
                      required></uui-input>
        </uui-form-layout-item>
      </ns-property>
      <ns-property
        label="ns_mailjet_sandboxMode"
        description="ns_mailjet_sandboxModeDescription"
        .isLastPropertyElement=${false}
        >
        <ns-checkbox .checked=${settings.mj_sandboxMode ?? false}
                    @change=${(e:Event)=>this.updateValue('mj_sandboxMode',(e.target as unknown as NsCheckboxElement).checked)}></ns-checkbox>
      </ns-property>
      <hr/>
      <h2>${this.localize.term('ns_mailjet_bounceManagementHeader')}</h2>
      <p>${unsafeHTML(this.localize.term('ns_mailjet_bounceManagementIntroduction'))}</p>
      ${when(this.configuration?.isBaseUrlLocalhost,
        ()=>html`<p>${this.localize.term('ns_mailjet_bounceManagementNotLocalhost')}</p>`,
        ()=>html`
          <p>There are two ways to activate this:</p>
          <h3>Automatic configuration</h3>
          <p>
              If the API key and secrets are configured we can configure the webhooks via Mailjet's API. Press the button to update the configuration.
          </p>
          <uui-button
            look="outline"
            color="default"
            label="Configure now"
            @click=${this.#handleConfigureNow}
            ></uui-button>
          <h3>Manual configuration</h3>
          <p>
              Login to Mailjet and go to settings, click on Event notifications (webhook) (<a href="https://app.mailjet.com/account/triggers" target="_blank">link</a>).<br />
              Enter the following URL for the Bounce, Spam and Blocked-events:
          </p>
          <pre>{{pVm.data.webhookUrl}}</pre>
        `)
      }

    `

  }

  static styles = [UmbTextStyles, css`

    uui-input {
      width:500px;
      max-width:100%;
    }
    uui-input[type=number] {
      width:100px;
    }
    h2 {
      margin:20px 0 10px 0;
    }
    p {
      margin:0;
    }
    p + p {
      margin-top:10px;
    }
    hr {
      height: 1px;
      border: none;
      border-top:1px solid var(--ns-color-divider)
    }

    uui-form-layout-item {
      margin:0;
    }

    pre {
      font-size:10px;
      background:#f5f5f5;
      border:1px solid #b8b8b8;
      padding:5px;
    }
  `]
}

export default NsEmailServiceSettingsSmtpElement;

declare global {
  interface HTMLElementTagNameMap {
    'ns-email-service-settings-mailjet': NsEmailServiceSettingsSmtpElement;
  }
}

interface MailjetProviderSettings {
  mj_apiKey? : string;
  mj_apiSecret? : string;
  mj_sandboxMode : boolean;
}
