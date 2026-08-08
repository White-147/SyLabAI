using Microsoft.Extensions.DependencyInjection;
using SyLabAI.Application.Runtime;
using SyLabAI.Infrastructure.SqlServer.Storage;

namespace SyLabAI.Infrastructure.SqlServer;

public static class SqlServerInfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddSyLabAISqlServerInfrastructure(this IServiceCollection services)
    {
        services.AddSingleton<ILabKnowledgeStore, SqlServerLabKnowledgeStore>();
        return services;
    }
}
