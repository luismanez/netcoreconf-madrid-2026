using Microsoft.Extensions.Configuration;

namespace WftEngineering.Demo;

internal sealed record DemoSettings(
    Uri FoundryProjectEndpoint,
    string FoundryModel,
    Uri OtlpEndpoint,
    string OtlpProtocol)
{
    public static DemoSettings FromConfiguration(IConfiguration configuration)
    {
        var endpoint = Required(configuration, "FOUNDRY_PROJECT_ENDPOINT");
        var model = Required(configuration, "FOUNDRY_MODEL");
        var otlpEndpoint = configuration["OTEL_EXPORTER_OTLP_ENDPOINT"]?.Trim();
        var protocol = configuration["OTEL_EXPORTER_OTLP_PROTOCOL"]?.Trim();

        if (string.IsNullOrWhiteSpace(otlpEndpoint))
        {
            otlpEndpoint = "http://localhost:4317";
        }

        if (string.IsNullOrWhiteSpace(protocol))
        {
            protocol = "grpc";
        }

        if (protocol != "grpc")
        {
            throw new DemoConfigurationException("OTEL_EXPORTER_OTLP_PROTOCOL must be grpc.");
        }

        return new DemoSettings(
            ParseEndpoint(endpoint, "FOUNDRY_PROJECT_ENDPOINT", httpsOnly: true),
            model,
            ParseEndpoint(otlpEndpoint, "OTEL_EXPORTER_OTLP_ENDPOINT", httpsOnly: false),
            protocol);
    }

    private static string Required(IConfiguration configuration, string key)
    {
        var value = configuration[key]?.Trim();
        return string.IsNullOrWhiteSpace(value)
            ? throw new DemoConfigurationException($"{key} is required.")
            : value;
    }

    private static Uri ParseEndpoint(string value, string key, bool httpsOnly)
    {
        if (!Uri.TryCreate(value, UriKind.Absolute, out var uri)
            || !string.IsNullOrEmpty(uri.UserInfo)
            || (uri.Scheme != Uri.UriSchemeHttps && (httpsOnly || uri.Scheme != Uri.UriSchemeHttp)))
        {
            var scheme = httpsOnly ? "HTTPS" : "HTTP(S)";
            throw new DemoConfigurationException($"{key} must be an absolute {scheme} URL without embedded credentials.");
        }

        return uri;
    }
}

internal sealed class DemoConfigurationException(string message) : Exception(message);
