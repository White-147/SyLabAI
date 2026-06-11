using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SyLabAI.Application.Settings;

namespace SyLabAI.Infrastructure.AI.Settings;

internal sealed class DeepSeekProviderSettingsService(
    IConfiguration configuration,
    LocalProviderSettingsStore localSettingsStore,
    HttpClient httpClient,
    ILogger<DeepSeekProviderSettingsService> logger) : IProviderSettingsService
{
    private const string DefaultProviderName = "DeepSeek";
    private const string DefaultBaseUrl = "https://api.deepseek.com";
    private const string DefaultModel = "deepseek-v4-pro";

    public ProviderStatus GetStatus()
    {
        var localSettings = localSettingsStore.Read();
        var apiKeyState = ResolveApiKey(localSettings);
        var baseUrl = ResolveBaseUrl(localSettings);
        var model = ResolveModel(localSettings);
        var liveCallsEnabled = ResolveLiveCallsEnabled(localSettings);
        var baseUrlAccepted = IsAcceptedBaseUrl(baseUrl);
        var configured = apiKeyState.ApiKey is not null && baseUrlAccepted && !string.IsNullOrWhiteSpace(model);
        var mode = ResolveMode(configured, liveCallsEnabled);

        return new ProviderStatus(
            configuration["SyLabAI:Provider:Name"] ?? DefaultProviderName,
            model,
            baseUrl,
            configured,
            apiKeyState.Source,
            mode,
            liveCallsEnabled,
            BuildSafetyGates(apiKeyState.Source, baseUrlAccepted, liveCallsEnabled));
    }

    public ProviderStatus SaveSettings(ProviderSettingsUpdate update)
    {
        var current = localSettingsStore.Read();
        var next = current with
        {
            BaseUrl = NormalizeBaseUrl(update.BaseUrl),
            Model = string.IsNullOrWhiteSpace(update.Model) ? current.Model : update.Model.Trim(),
            LiveCallsEnabled = update.LiveCallsEnabled,
            ProtectedApiKey = string.IsNullOrWhiteSpace(update.ApiKey)
                ? current.ProtectedApiKey
                : localSettingsStore.ProtectApiKey(update.ApiKey.Trim())
        };

        localSettingsStore.Save(next);
        logger.LogInformation(
            "DeepSeek provider settings saved. Host={Host}; Model={Model}; ApiKeyUpdated={ApiKeyUpdated}; LiveCallsEnabled={LiveCallsEnabled}",
            TryGetHost(next.BaseUrl),
            ResolveModel(next),
            !string.IsNullOrWhiteSpace(update.ApiKey),
            next.LiveCallsEnabled);

        return GetStatus();
    }

    public ProviderStatus ClearApiKey()
    {
        var current = localSettingsStore.Read();
        localSettingsStore.Save(current with { ProtectedApiKey = null });
        logger.LogInformation("DeepSeek local provider API key cleared.");

        return GetStatus();
    }

    public async Task<ProviderConnectivityTestResult> TestConnectivityAsync(CancellationToken cancellationToken)
    {
        var modelResult = await ListModelsAsync(cancellationToken);
        return new ProviderConnectivityTestResult(
            modelResult.Status,
            modelResult.Message,
            modelResult.HttpStatusCode,
            modelResult.CheckedAt);
    }

    public async Task<ProviderModelListResult> ListModelsAsync(CancellationToken cancellationToken)
    {
        var localSettings = localSettingsStore.Read();
        var apiKeyState = ResolveApiKey(localSettings);
        if (apiKeyState.ApiKey is null)
        {
            return CreateModelListResult("not_configured", "API key is not configured.", null, []);
        }

        var baseUrl = ResolveBaseUrl(localSettings);
        if (!IsAcceptedBaseUrl(baseUrl))
        {
            return CreateModelListResult("invalid_base_url", "Base URL must be an HTTPS absolute URL.", null, []);
        }

        try
        {
            var response = await FetchModelsAsync(baseUrl, apiKeyState.ApiKey, cancellationToken);
            var result = ClassifyModelListResponse(response.StatusCode, response.Body);

            logger.LogInformation(
                "DeepSeek provider models request completed. Status={Status}; HttpStatusCode={HttpStatusCode}; ModelCount={ModelCount}",
                result.Status,
                result.HttpStatusCode,
                result.Models.Count);

            return result;
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            return CreateModelListResult("timeout", "Provider models request timed out.", null, []);
        }
        catch (HttpRequestException)
        {
            return CreateModelListResult("unreachable", "Provider address is unreachable.", null, []);
        }
    }

    private string ResolveBaseUrl(LocalProviderSettings localSettings)
    {
        return NormalizeBaseUrl(localSettings.BaseUrl ?? configuration["SyLabAI:Provider:BaseUrl"] ?? DefaultBaseUrl);
    }

    private string ResolveModel(LocalProviderSettings localSettings)
    {
        return (localSettings.Model ?? configuration["SyLabAI:Provider:Model"] ?? DefaultModel).Trim();
    }

    private bool ResolveLiveCallsEnabled(LocalProviderSettings localSettings)
    {
        return localSettings.LiveCallsEnabled ?? configuration.GetValue("SyLabAI:Provider:EnableLiveCalls", false);
    }

    private ApiKeyState ResolveApiKey(LocalProviderSettings localSettings)
    {
        var configuredKey = configuration["SyLabAI:Provider:ApiKey"];
        if (!string.IsNullOrWhiteSpace(configuredKey))
        {
            return new ApiKeyState(configuredKey, "configuration:SyLabAI:Provider:ApiKey");
        }

        var deepSeekEnv = configuration["DEEPSEEK_API_KEY"];
        if (!string.IsNullOrWhiteSpace(deepSeekEnv))
        {
            return new ApiKeyState(deepSeekEnv, "environment:DEEPSEEK_API_KEY");
        }

        var sylabEnv = configuration["SYLABAI_PROVIDER_API_KEY"];
        if (!string.IsNullOrWhiteSpace(sylabEnv))
        {
            return new ApiKeyState(sylabEnv, "environment:SYLABAI_PROVIDER_API_KEY");
        }

        var localKey = localSettingsStore.UnprotectApiKey(localSettings);
        if (!string.IsNullOrWhiteSpace(localKey))
        {
            return new ApiKeyState(localKey, "local-protected-file");
        }

        return new ApiKeyState(null, "none");
    }

    private async Task<ProviderModelsResponse> FetchModelsAsync(
        string baseUrl,
        string apiKey,
        CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, BuildEndpoint(baseUrl, "models"));
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        using var response = await httpClient.SendAsync(request, cancellationToken);
        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        return new ProviderModelsResponse(response.StatusCode, body);
    }

    private static ProviderModelListResult ClassifyModelListResponse(HttpStatusCode statusCode, string body)
    {
        var code = (int)statusCode;
        if (statusCode is >= HttpStatusCode.OK and < HttpStatusCode.MultipleChoices)
        {
            var models = ParseModels(body);
            if (models.Count == 0)
            {
                return CreateModelListResult("empty_models", "Provider is reachable but no model IDs were returned.", code, []);
            }

            return CreateModelListResult("connected", "Provider models loaded.", code, models);
        }

        if (statusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden)
        {
            return CreateModelListResult("auth_failed", "API key is invalid or unauthorized.", code, []);
        }

        if (statusCode == HttpStatusCode.PaymentRequired || ContainsBalanceError(body))
        {
            return CreateModelListResult("insufficient_balance", "API key is reachable, but balance or quota is insufficient.", code, []);
        }

        if (statusCode == HttpStatusCode.TooManyRequests)
        {
            return CreateModelListResult("rate_limited", "Provider returned a rate limit response.", code, []);
        }

        return CreateModelListResult("provider_error", "Provider returned a non-success status.", code, []);
    }

    private static IReadOnlyList<ProviderModelOption> ParseModels(string body)
    {
        try
        {
            using var document = JsonDocument.Parse(body);
            if (!document.RootElement.TryGetProperty("data", out var data) || data.ValueKind != JsonValueKind.Array)
            {
                return [];
            }

            return data
                .EnumerateArray()
                .Select(model =>
                {
                    var id = model.TryGetProperty("id", out var idValue) && idValue.ValueKind == JsonValueKind.String
                        ? idValue.GetString()
                        : null;
                    var ownedBy = model.TryGetProperty("owned_by", out var ownedByValue) && ownedByValue.ValueKind == JsonValueKind.String
                        ? ownedByValue.GetString()
                        : DefaultProviderName;

                    return string.IsNullOrWhiteSpace(id)
                        ? null
                        : new ProviderModelOption(
                            id.Trim(),
                            string.IsNullOrWhiteSpace(ownedBy) ? DefaultProviderName : ownedBy.Trim());
                })
                .Where(model => model is not null)
                .Cast<ProviderModelOption>()
                .GroupBy(model => model.Id, StringComparer.OrdinalIgnoreCase)
                .Select(group => group.First())
                .OrderBy(model => model.Id, StringComparer.OrdinalIgnoreCase)
                .ToArray();
        }
        catch (JsonException)
        {
            return [];
        }
    }

    private static bool ContainsBalanceError(string body)
    {
        return body.Contains("insufficient", StringComparison.OrdinalIgnoreCase)
            || body.Contains("balance", StringComparison.OrdinalIgnoreCase)
            || body.Contains("quota", StringComparison.OrdinalIgnoreCase)
            || body.Contains("余额", StringComparison.OrdinalIgnoreCase);
    }

    private static ProviderModelListResult CreateModelListResult(
        string status,
        string message,
        int? httpStatusCode,
        IReadOnlyList<ProviderModelOption> models)
    {
        return new ProviderModelListResult(status, message, httpStatusCode, models, DateTimeOffset.UtcNow);
    }

    private static string ResolveMode(bool configured, bool liveCallsEnabled)
    {
        if (!configured)
        {
            return "missing-api-key";
        }

        return liveCallsEnabled ? "configured-live-gated" : "configured-dry-run";
    }

    private static IReadOnlyList<string> BuildSafetyGates(
        string apiKeySource,
        bool baseUrlAccepted,
        bool liveCallsEnabled)
    {
        return
        [
            "Provider uses the OpenAI-compatible DeepSeek API boundary.",
            "API keys are read from configuration, environment variables, or the local protected settings file.",
            "API key value is never returned by DTOs.",
            baseUrlAccepted ? "Provider base URL uses HTTPS." : "Provider base URL is not accepted.",
            liveCallsEnabled ? "Live calls are enabled behind explicit gates." : "Live calls are disabled for generation paths.",
            apiKeySource == "none" ? "No API key source is configured." : $"API key source detected: {apiKeySource}."
        ];
    }

    private static string NormalizeBaseUrl(string value)
    {
        return value.Trim().TrimEnd('/');
    }

    private static bool IsAcceptedBaseUrl(string value)
    {
        return Uri.TryCreate(value, UriKind.Absolute, out var uri) && uri.Scheme == Uri.UriSchemeHttps;
    }

    private static Uri BuildEndpoint(string baseUrl, string relativePath)
    {
        return new Uri($"{baseUrl.TrimEnd('/')}/{relativePath.TrimStart('/')}");
    }

    private static string TryGetHost(string? baseUrl)
    {
        return Uri.TryCreate(baseUrl, UriKind.Absolute, out var uri) ? uri.Host : "invalid";
    }

    private sealed record ApiKeyState(string? ApiKey, string Source);

    private sealed record ProviderModelsResponse(HttpStatusCode StatusCode, string Body);
}
