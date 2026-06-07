using SyLabAI.Domain.Knowledge;

namespace SyLabAI.Domain.Documents;

public sealed record DocumentChunk(
    Guid Id,
    Guid DocumentId,
    string DocumentTitle,
    string Section,
    int Ordinal,
    string Text,
    SourceCitation Citation);

