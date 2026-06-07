using SyLabAI.Domain.Knowledge;

namespace SyLabAI.Domain.Experiments;

public sealed record StructuredExperimentRecord(
    Guid Id,
    string Title,
    IReadOnlyDictionary<string, string> Conditions,
    IReadOnlyDictionary<string, string> Results,
    IReadOnlyList<string> Observations,
    IReadOnlyList<SearchHit> Evidence,
    bool RequiresHumanReview);

