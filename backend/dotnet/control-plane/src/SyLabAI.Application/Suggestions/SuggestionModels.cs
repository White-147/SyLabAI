using SyLabAI.Domain.Suggestions;

namespace SyLabAI.Application.Suggestions;

public sealed record PathSuggestionRequest(
    string Objective,
    string Constraints);

public interface IPathSuggestionService
{
    Task<PathSuggestionDraft> CreateDraftAsync(PathSuggestionRequest request, CancellationToken cancellationToken);
}

