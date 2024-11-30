// This file is auto-generated by @hey-api/openapi-ts

export type CheckWebhookConfigurationRequestModel = {
    settings: {
        [key: string]: unknown;
    };
    workspaceKey: string;
    baseUrl: string;
};

export type CheckWebhookConfigurationResponseModel = {
    webhookUrl: string;
    isBaseUrlLocalhost: boolean;
};

export type EventMessageTypeModel = 'Default' | 'Info' | 'Error' | 'Success' | 'Warning';

export const EventMessageTypeModel = {
    DEFAULT: 'Default',
    INFO: 'Info',
    ERROR: 'Error',
    SUCCESS: 'Success',
    WARNING: 'Warning'
} as const;

export type NotificationHeaderModel = {
    message: string;
    category: string;
    type: EventMessageTypeModel;
};

export type ConfigureNowData = {
    requestBody?: CheckWebhookConfigurationRequestModel;
};

export type ConfigureNowResponse = CheckWebhookConfigurationResponseModel;

export type GetConfigurationData = {
    requestBody?: CheckWebhookConfigurationRequestModel;
};

export type GetConfigurationResponse = CheckWebhookConfigurationResponseModel;

export type $OpenApiTs = {
    '/umbraco/management/api/newsletter-studio-mailjet/configure-now': {
        post: {
            req: ConfigureNowData;
            res: {
                /**
                 * OK
                 */
                200: CheckWebhookConfigurationResponseModel;
                /**
                 * The resource is protected and requires an authentication token
                 */
                401: unknown;
            };
        };
    };
    '/umbraco/management/api/newsletter-studio-mailjet/get-configuration': {
        post: {
            req: GetConfigurationData;
            res: {
                /**
                 * OK
                 */
                200: CheckWebhookConfigurationResponseModel;
                /**
                 * The resource is protected and requires an authentication token
                 */
                401: unknown;
            };
        };
    };
};