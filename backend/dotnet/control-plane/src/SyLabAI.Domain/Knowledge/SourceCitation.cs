namespace SyLabAI.Domain.Knowledge;

public sealed record SourceCitation(
    Guid DocumentId,
    Guid ChunkId,
    string DocumentTitle,
    string Section,
    int ChunkOrdinal);

