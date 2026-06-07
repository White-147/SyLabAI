using SyLabAI.Application.Suggestions;
using SyLabAI.ControlApi.Contracts;

namespace SyLabAI.ControlApi.Modules.Suggestions;

internal static class SuggestionEndpoints
{
    public static IEndpointRouteBuilder MapSuggestionEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/suggestions").WithTags("Suggestions");

        group.MapPost("/", async (
            PathSuggestionDto request,
            IPathSuggestionService service,
            CancellationToken cancellationToken) =>
        {
            if (string.IsNullOrWhiteSpace(request.objective))
            {
                return Results.BadRequest(new ValidationErrorDto("Suggestion objective is required."));
            }

            var draft = await service.CreateDraftAsync(
                new PathSuggestionRequest(request.objective, request.constraints ?? string.Empty),
                cancellationToken);

            return Results.Ok(draft.ToDto());
        });

        return app;
    }
}

