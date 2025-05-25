import type { UmbEntryPointOnInit } from '@umbraco-cms/backoffice/extension-api';
import { UMB_AUTH_CONTEXT } from '@umbraco-cms/backoffice/auth';
import { registerManifest } from './manifest.js';
import { client } from './backend-api/client.gen.js';

export const onInit: UmbEntryPointOnInit = (host, extensionRegistry) => {

  host.consumeContext(UMB_AUTH_CONTEXT,(authContext)=> {

    if(!authContext)
      return;

    const config = authContext!.getOpenApiConfiguration();

    client.setConfig({
      auth: config.token,
      baseUrl: config.base,
      credentials: config.credentials,
    });

  });

  registerManifest(extensionRegistry);
};

