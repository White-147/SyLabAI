using SyLabAI.Application.Experiments;
using SyLabAI.ControlApi.Contracts;

namespace SyLabAI.ControlApi.Modules.Experiments;

internal static class ExperimentEndpoints
{
    public static IEndpointRouteBuilder MapExperimentEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/experiments").WithTags("Experiments");

        group.MapPost("/extractions", async (
            ExperimentExtractionDto request,
            IExperimentExtractionService service,
            CancellationToken cancellationToken) =>
        {
            if (string.IsNullOrWhiteSpace(request.title) || string.IsNullOrWhiteSpace(request.rawNote))
            {
                return Results.BadRequest(new ValidationErrorDto("Title and raw note are required."));
            }

            var record = await service.ExtractAsync(
                new ExperimentExtractionRequest(request.title, request.rawNote),
                cancellationToken);

            return Results.Ok(record.ToDto());
        });

        return app;
    }
}

