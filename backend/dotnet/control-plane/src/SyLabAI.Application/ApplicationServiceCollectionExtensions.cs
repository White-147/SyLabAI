using Microsoft.Extensions.DependencyInjection;
using SyLabAI.Application.Documents;
using SyLabAI.Application.Experiments;
using SyLabAI.Application.Knowledge;
using SyLabAI.Application.Suggestions;
using SyLabAI.Application.Tasks;

namespace SyLabAI.Application;

public static class ApplicationServiceCollectionExtensions
{
    public static IServiceCollection AddSyLabAIApplication(this IServiceCollection services)
    {
        services.AddScoped<IDocumentIngestionService, DocumentIngestionService>();
        services.AddScoped<IKnowledgeService, KnowledgeService>();
        services.AddScoped<IExperimentExtractionService, ExperimentExtractionService>();
        services.AddScoped<IPathSuggestionService, PathSuggestionService>();
        services.AddScoped<ILabTaskService, LabTaskService>();

        return services;
    }
}

