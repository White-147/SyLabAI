using Microsoft.Extensions.DependencyInjection;
using SyLabAI.Application.Documents;
using SyLabAI.Infrastructure.Documents.Chunking;

namespace SyLabAI.Infrastructure.Documents;

public static class DocumentInfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddSyLabAIDocumentInfrastructure(this IServiceCollection services)
    {
        services.AddSingleton<IDocumentChunker, SimpleDocumentChunker>();
        return services;
    }
}

