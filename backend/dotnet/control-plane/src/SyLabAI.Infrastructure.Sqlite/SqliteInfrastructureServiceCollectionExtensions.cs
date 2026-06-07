using Microsoft.Extensions.DependencyInjection;
using SyLabAI.Application.Runtime;
using SyLabAI.Infrastructure.Sqlite.DemoStore;

namespace SyLabAI.Infrastructure.Sqlite;

public static class SqliteInfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddSyLabAISqliteInfrastructure(this IServiceCollection services)
    {
        services.AddSingleton<ILabKnowledgeStore, DemoLabKnowledgeStore>();
        return services;
    }
}

