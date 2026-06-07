using SyLabAI.Application.Knowledge;
using SyLabAI.ControlApi.Contracts;

namespace SyLabAI.ControlApi.Modules.Knowledge;

internal static class KnowledgeEndpoints
{
    public static IEndpointRouteBuilder MapKnowledgeEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/knowledge").WithTags("Knowledge");

        group.MapPost("/search", async (
            KnowledgeSearchDto request,
            IKnowledgeService service,
            CancellationToken cancellationToken) =>
        {
            if (string.IsNullOrWhiteSpace(request.query))
            {
                return Results.BadRequest(new ValidationErrorDto("Search query is required."));
            }

            var results = await service.SearchAsync(
                new KnowledgeSearchRequest(request.query, request.limit ?? 5),
                cancellationToken);

            return Results.Ok(results.Select(result => result.ToDto()).ToArray());
        });

        group.MapPost("/answers", async (
            KnowledgeAnswerDto request,
            IKnowledgeService service,
            CancellationToken cancellationToken) =>
        {
            if (string.IsNullOrWhiteSpace(request.question))
            {
                return Results.BadRequest(new ValidationErrorDto("Question is required."));
            }

            var answer = await service.AnswerAsync(
                new KnowledgeAnswerRequest(request.question, request.evidenceLimit ?? 4),
                cancellationToken);

            return Results.Ok(answer.ToDto());
        });

        return app;
    }
}

