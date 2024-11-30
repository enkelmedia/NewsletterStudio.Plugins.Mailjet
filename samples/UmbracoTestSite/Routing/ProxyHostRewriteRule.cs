using Microsoft.AspNetCore.Rewrite;

namespace UmbracoTestSite.Routing;

/// <summary>
/// Rewrite rule that will to use when running behind a proxy.
/// This will "fool" the application that the incoming URL is the url from x-forwarded-host and x-forwarded-scheme
/// </summary>
/// <remarks>
/// Read more here: https://github.com/umbraco/Umbraco-CMS/issues/16179
/// </remarks>
public class ProxyHostRewriteRule : Microsoft.AspNetCore.Rewrite.IRule
{
    public void ApplyRule(RewriteContext context)
    {
        context.Result = RuleResult.ContinueRules;

        // Get a reference to the current request
        HttpRequest request = context.HttpContext.Request;

        // Get the host from the header
        string? proxyHost = request.Headers["x-forwarded-host"];
        if (!string.IsNullOrWhiteSpace(proxyHost))
        {
            // Update the host
            request.Host = new HostString(proxyHost);
        }

        string? proxyScheme = request.Headers["x-forwarded-scheme"];
        if (!string.IsNullOrWhiteSpace(proxyScheme))
        {
            // Update the scheme (http/https)
            request.Scheme = proxyScheme;
        }

    }
}

public static class ProxyHostStartupExtensions
{
    public static WebApplication UseProxyHostRewrite(this WebApplication app)
    {
        var rewriteOptions = new RewriteOptions();
        rewriteOptions.Rules.Add(new ProxyHostRewriteRule());
        app.UseRewriter(rewriteOptions);
        return app;
    }
}


