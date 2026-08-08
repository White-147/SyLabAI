using System.Text.Json;
using SyLabAI.Application;
using SyLabAI.ControlApi.Modules.Documents;
using SyLabAI.ControlApi.Modules.Experiments;
using SyLabAI.ControlApi.Modules.Health;
using SyLabAI.ControlApi.Modules.Knowledge;
using SyLabAI.ControlApi.Modules.LabTasks;
using SyLabAI.ControlApi.Modules.Settings;
using SyLabAI.ControlApi.Modules.Suggestions;
using SyLabAI.Infrastructure.AI;
using SyLabAI.Infrastructure.Documents;
using SyLabAI.Infrastructure.SqlServer;

var builder = WebApplication.CreateBuilder(args);

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("SyLabAI.Web", policy =>
    {
        policy
            .WithOrigins(
                "http://localhost:3000",
                "http://127.0.0.1:3000",
                "http://localhost:5173",
                "http://127.0.0.1:5173")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services
    .AddSyLabAIApplication()
    .AddSyLabAIAiInfrastructure()
    .AddSyLabAIDocumentInfrastructure()
    .AddSyLabAISqlServerInfrastructure();

var app = builder.Build();

app.UseCors("SyLabAI.Web");

app
    .MapHealthEndpoints()
    .MapDocumentEndpoints()
    .MapKnowledgeEndpoints()
    .MapExperimentEndpoints()
    .MapSuggestionEndpoints()
    .MapLabTaskEndpoints()
    .MapSettingsEndpoints();

app.Run();

public partial class Program;
