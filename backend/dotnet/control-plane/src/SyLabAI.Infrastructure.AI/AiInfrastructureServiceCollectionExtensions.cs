using Microsoft.Extensions.DependencyInjection;
using SyLabAI.Application.Settings;
using SyLabAI.Infrastructure.AI.Settings;

namespace SyLabAI.Infrastructure.AI;

public static class AiInfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddSyLabAIAiInfrastructure(this IServiceCollection services)
    {
        services.AddSingleton<LocalProviderSettingsStore>();
        services
            .AddHttpClient<IProviderSettingsService, DeepSeekProviderSettingsService>(client =>
            {
                client.Timeout = TimeSpan.FromSeconds(12);
            });
        return services;
    }
}
