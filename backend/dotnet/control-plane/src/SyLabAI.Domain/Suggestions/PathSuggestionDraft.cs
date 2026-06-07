using SyLabAI.Domain.Knowledge;

namespace SyLabAI.Domain.Suggestions;

public sealed record PathSuggestionDraft(
    Guid Id,
    string Objective,
    IReadOnlyList<string> ProposedSteps,
    IReadOnlyList<string> Assumptions,
    IReadOnlyList<string> Risks,
    IReadOnlyList<SearchHit> Evidence,
    bool RequiresHumanReview);

