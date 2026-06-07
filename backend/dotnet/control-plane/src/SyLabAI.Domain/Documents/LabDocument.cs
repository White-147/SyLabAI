namespace SyLabAI.Domain.Documents;

public sealed record LabDocument(
    Guid Id,
    string Title,
    string DocumentType,
    string Status,
    string Summary,
    DateTimeOffset CreatedAt,
    IReadOnlyList<DocumentChunk> Chunks);

