namespace SyLabAI.Application.Settings;

public sealed record ProviderStatus(
    string Provider,
    string Model,
    string BaseUrl,
    bool Configured,
    string ApiKeySource,
    string Mode,
    bool LiveCallsEnabled,
    IReadOnlyList<string> SafetyGates);

public sealed record ProviderSettingsUpdate(
    string BaseUrl,
    string Model,
    string? ApiKey,
    bool LiveCallsEnabled);

public sealed record ProviderConnectivityTestResult(
    string Status,
    string Message,
    int? HttpStatusCode,
    DateTimeOffset CheckedAt);

public sealed record ProviderModelOption(
    string Id,
    string OwnedBy);

public sealed record ProviderModelListResult(
    string Status,
    string Message,
    int? HttpStatusCode,
    IReadOnlyList<ProviderModelOption> Models,
    DateTimeOffset CheckedAt);

public interface IProviderSettingsService
{
    ProviderStatus GetStatus();

    ProviderStatus SaveSettings(ProviderSettingsUpdate update);

    ProviderStatus ClearApiKey();

    Task<ProviderConnectivityTestResult> TestConnectivityAsync(CancellationToken cancellationToken);

    Task<ProviderModelListResult> ListModelsAsync(CancellationToken cancellationToken);
}
