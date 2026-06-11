using Microsoft.Extensions.DependencyInjection;
using SyLabAI.Application.Runtime;
using SyLabAI.Infrastructure.PostgreSql.Storage;

namespace SyLabAI.Infrastructure.PostgreSql;

public static class PostgreSqlInfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddSyLabAIPostgreSqlInfrastructure(this IServiceCollection services)
    {
        services.AddSingleton<ILabKnowledgeStore, PostgreSqlLabKnowledgeStore>();
        return services;
    }
}
