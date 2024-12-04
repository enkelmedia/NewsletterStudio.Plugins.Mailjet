using Asp.Versioning;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using NewsletterStudio.Web.Controllers.Api;
using Swashbuckle.AspNetCore.SwaggerGen;
using Umbraco.Cms.Api.Common.OpenApi;
using Umbraco.Cms.Api.Management.OpenApi;
using Umbraco.Extensions;

namespace NewsletterStudio.Plugins.Mailjet.Backoffice.Api;

public static class NewsletterStudioPluginApiConfiguration
{
    public const string ApiName = "newsletter-studio-plugin";
    public const string ApiTitle = "Newsletter Studio Plugin";
}

internal class NewsletterStudioPluginSchemaIdHandler : SchemaIdHandler
{
    public override bool CanHandle(Type type)
    {
        if (type.Namespace?.StartsWith("NewsletterStudio") is true)
            return true;

        return false;

    }
}

public class NewsletterStudioPluginOperationIdHandler : OperationIdHandler
{
    public NewsletterStudioPluginOperationIdHandler(IOptions<ApiVersioningOptions> apiVersioningOptions) : base(apiVersioningOptions)
    {
    }

    protected override bool CanHandle(ApiDescription apiDescription, ControllerActionDescriptor controllerActionDescriptor)
        => controllerActionDescriptor.ControllerTypeInfo.Namespace?.StartsWith("NewsletterStudio") == true;

    public override string Handle(ApiDescription apiDescription)
    {
        if (apiDescription.ActionDescriptor is ControllerActionDescriptor ctrlActionDescriptor)
        {
            return ctrlActionDescriptor.ActionName.ToFirstLower();
        }

        return $"{apiDescription.ActionDescriptor.RouteValues["action"]}";
    }
}


public class ConfigureNewsletterStudioPluginApiSwaggerGenOptions : IConfigureOptions<SwaggerGenOptions>
{
    public void Configure(SwaggerGenOptions swaggerGenOptions)
    {
        swaggerGenOptions.SwaggerDoc(
            NewsletterStudioPluginApiConfiguration.ApiName,
            new OpenApiInfo
            {
                Title = NewsletterStudioPluginApiConfiguration.ApiTitle,
                Version = "1.0",
                Description = $"Backoffice API for Newsletter Studio Plugin, for our internal use contracts might break at any time."
            });

        //swaggerGenOptions.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, $"{Constants.PackageId}.xml"));

        //swaggerGenOptions.CustomOperationIds(e =>
        //{
        //    if (e.ActionDescriptor is ControllerActionDescriptor ctrlActionDescriptor)
        //    {
        //        return $"{ctrlActionDescriptor.ControllerName}{ctrlActionDescriptor.RouteValues["action"]}";
        //    }

        //    return $"{e.ActionDescriptor.RouteValues["action"]}";

        //});

        swaggerGenOptions.OperationFilter<MyItemApiOperationSecurityFilter>();

        //swaggerGenOptions.CustomOperationIds(e =>
        //{
        //    if (e.ActionDescriptor is ControllerActionDescriptor ctrlActionDescriptor)
        //    {
        //        return $"{ctrlActionDescriptor.ControllerName}{ctrlActionDescriptor.RouteValues["action"]}";
        //    }

        //    return $"{e.ActionDescriptor.RouteValues["action"]}";

        //});

    }
}

public class MyItemApiOperationSecurityFilter : BackOfficeSecurityRequirementsOperationFilterBase
{
    protected override string ApiName => NewsletterStudioPluginApiConfiguration.ApiName;
}
