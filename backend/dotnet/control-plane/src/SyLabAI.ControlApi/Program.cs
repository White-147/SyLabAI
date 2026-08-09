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
using SyLabAI.Infrastructure.Sqlite;

var builder = WebApplication.CreateBuilder(args);

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
});

// 允许源：本地开发三件套 + 环境变量 SYLABAI_CORS_ORIGINS（逗号分隔，追加线上前端域名）
var allowedOrigins = new List<string>
{
    "http://localhost:3000",
    "http://127.0.0.1:3000",
    "http://localhost:5173",
    "http://127.0.0.1:5173",
};
var extraOrigins = builder.Configuration["SYLABAI_CORS_ORIGINS"];
if (!string.IsNullOrWhiteSpace(extraOrigins))
{
    allowedOrigins.AddRange(extraOrigins.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));
}

builder.Services.AddCors(options =>
{
    options.AddPolicy("SyLabAI.Web", policy =>
    {
        policy
            .WithOrigins(allowedOrigins.ToArray())
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

// 存储层：SyLabAI:StorageProvider = Sqlite（默认，免装 SQL Server，适合容器部署）| SqlServer（本地 Windows 使用）
var storageProvider = builder.Configuration["SyLabAI:StorageProvider"] ?? "Sqlite";
builder.Services
    .AddSyLabAIApplication()
    .AddSyLabAIAiInfrastructure()
    .AddSyLabAIDocumentInfrastructure();

if (storageProvider.Equals("Sqlite", StringComparison.OrdinalIgnoreCase))
{
    builder.Services.AddSyLabAISqliteInfrastructure();
}
else
{
    builder.Services.AddSyLabAISqlServerInfrastructure();
}

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
