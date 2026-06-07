namespace SyLabAI.Application.Settings;

public sealed record ProviderStatus(
    string Provider,
    string Model,
    bool Configured,
    string Mode,
    IReadOnlyList<string> SafetyGates);

public interface IProviderStatusService
{
    ProviderStatus GetStatus();
}

