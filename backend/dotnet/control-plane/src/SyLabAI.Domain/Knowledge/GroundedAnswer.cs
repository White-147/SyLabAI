namespace SyLabAI.Domain.Knowledge;

public sealed record GroundedAnswer(
    string Answer,
    IReadOnlyList<SearchHit> Evidence,
    IReadOnlyList<string> Caveats,
    bool RequiresHumanReview);

