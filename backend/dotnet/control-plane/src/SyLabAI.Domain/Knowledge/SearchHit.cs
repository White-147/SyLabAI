namespace SyLabAI.Domain.Knowledge;

public sealed record SearchHit(
    SourceCitation Citation,
    string Snippet,
    double Score);

