using SyLabAI.Domain.Documents;

namespace SyLabAI.Application.Documents;

public interface IDocumentChunker
{
    IReadOnlyList<DocumentChunk> Chunk(Guid documentId, string documentTitle, string content);
}

