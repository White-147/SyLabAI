using SyLabAI.ControlApi.Contracts;

namespace SyLabAI.ControlApi.Modules.Health;

internal static class HealthEndpoints
{
    public static IEndpointRouteBuilder MapHealthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/health").WithTags("Health");

        group.MapGet("/", () =>
        {
            var response = new HealthDto(
                "ready",
                "SyLabAI.ControlApi",
                typeof(Program).Assembly.GetName().Version?.ToString() ?? "0.0.0",
                ["data", "uploads", "outputs", ".cache", ".config", ".tmp"],
                DateTimeOffset.UtcNow);

            return Results.Ok(response);
        });

        return app;
    }
}

