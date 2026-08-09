using Microsoft.Extensions.DependencyInjection;
using SyLabAI.Application.Runtime;
using SyLabAI.Infrastructure.Sqlite.Storage;

namespace SyLabAI.Infrastructure.Sqlite;

public static class SqliteInfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddSyLabAISqliteInfrastructure(this IServiceCollection services)
    {
        services.AddSingleton<ILabKnowledgeStore, SqliteLabKnowledgeStore>();
        return services;
    }
}
