import { defineConfig,defaultPlugins } from '@hey-api/openapi-ts';

export default defineConfig({
	input: 'http://localhost:54108/umbraco/swagger/newsletter-studio-plugin/swagger.json',
	output: {
		path: './src/backend-api',
    format:'prettier',
    lint:'eslint'
	},

	plugins: [
    ...defaultPlugins,
		{
			name: '@hey-api/client-fetch',
			bundle: false,
			exportFromIndex: true,
			throwOnError: true,
		},
		{
			name: '@hey-api/typescript',
			enums: 'typescript',
      readOnlyWriteOnlyBehavior : 'off'
		},
		{
			name: '@hey-api/sdk',
			asClass: true,
      serviceNameBuilder : '{{name}}Resource',
		}
	]
});
