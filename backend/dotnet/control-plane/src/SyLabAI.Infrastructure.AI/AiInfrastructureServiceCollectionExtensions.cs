using Microsoft.Extensions.DependencyInjection;
using SyLabAI.Application.Settings;
using SyLabAI.Infrastructure.AI.Settings;

namespace SyLabAI.Infrastructure.AI;

public static class AiInfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddSyLabAIAiInfrastructure(this IServiceCollection services)
    {
        services.AddSingleton<IProviderStatusService, DemoProviderStatusService>();
        return services;
    }
}

