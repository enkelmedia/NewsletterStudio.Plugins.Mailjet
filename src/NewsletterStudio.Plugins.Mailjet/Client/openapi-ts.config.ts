import { defineConfig } from "@hey-api/openapi-ts";

export default defineConfig({
  input: "http://localhost:54108/umbraco/openapi/newsletter-studio-plugin.json",
  output: {
    path: "./src/backend-api",
  },
  plugins: [
    {
      name: "@hey-api/client-fetch",

      // configure to re-export from index.ts
      exportFromIndex: true,

      // setting to throw since we want to trigger Umbraco's error handling to get a UmbApiError
      // with problemDetails back from tryExecute.
      throwOnError: true,
    },
    {
      name: "@hey-api/typescript",
      enums: "typescript",
    },
    {
      name: "@hey-api/sdk",
      operations: {
        strategy: "byTags",
        container: "class",
        containerName: "{{name}}Resource",
      },
    },
  ],
});
