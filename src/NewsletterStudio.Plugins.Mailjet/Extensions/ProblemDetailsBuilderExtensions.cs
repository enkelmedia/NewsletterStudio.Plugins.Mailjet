using Umbraco.Cms.Api.Common.Builders;

namespace NewsletterStudio.Plugins.Mailjet.Extensions;

internal static class ProblemDetailsBuilderExtensions
{
    internal static ProblemDetailsBuilder WithErrorDetails(this ProblemDetailsBuilder builder, string details)
    {
        return builder.WithRequestModelErrors(new Dictionary<string, string[]>() {{"details", [details]}});

    }
}
