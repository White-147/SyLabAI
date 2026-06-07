using Microsoft.Extensions.Configuration;
using SyLabAI.Application.Settings;

namespace SyLabAI.Infrastructure.AI.Settings;

internal sealed class DemoProviderStatusService(IConfiguration configuration) : IProviderStatusService
{
    public ProviderStatus GetStatus()
    {
        var provider = configuration["SyLabAI:Provider:Name"] ?? "DeepSeek";
        var model = configuration["SyLabAI:Provider:Model"] ?? "deepseek-chat";
        var hasKey = !string.IsNullOrWhiteSpace(configuration["SyLabAI:Provider:ApiKey"]);

        return new ProviderStatus(
            provider,
            model,
            hasKey,
            hasKey ? "configured-dry-run" : "demo-no-live-calls",
            [
                "Live provider calls require explicit developer intent.",
                "Prompts must be sanitized before leaving the intranet boundary.",
                "Raw provider payloads and prompts are not exposed through public DTOs."
            ]);
    }
}

