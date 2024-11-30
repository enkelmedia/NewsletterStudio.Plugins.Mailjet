import { defineConfig } from '@hey-api/openapi-ts';

export default defineConfig({
  client : 'fetch',
  input: 'http://localhost:54108/umbraco/swagger/newsletter-studio-plugin/swagger.json',
  output: {
    path :'src/backend-api',
    format:'prettier',
    lint : 'eslint'
  },
  schemas : false,
  services: {
    asClass : true,
    name: '{{name}}Resource',
  },
  types: {
    enums : 'javascript', // Typescript not recommended https://heyapi.vercel.app/openapi-ts/configuration.html#enums
  },
  debug: true
});
