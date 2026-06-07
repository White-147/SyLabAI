using SyLabAI.Application.Documents;
using SyLabAI.Domain.Documents;
using SyLabAI.Domain.Knowledge;

namespace SyLabAI.Infrastructure.Documents.Chunking;

internal sealed class SimpleDocumentChunker : IDocumentChunker
{
    private const int TargetChunkLength = 520;

    public IReadOnlyList<DocumentChunk> Chunk(Guid documentId, string documentTitle, string content)
    {
        var normalized = string.Join(' ', content.Split(default(char[]), StringSplitOptions.RemoveEmptyEntries));
        if (normalized.Length == 0)
        {
            return [];
        }

        var chunks = new List<DocumentChunk>();
        var ordinal = 1;

        for (var offset = 0; offset < normalized.Length; offset += TargetChunkLength)
        {
            var length = Math.Min(TargetChunkLength, normalized.Length - offset);
            var chunkId = Guid.NewGuid();
            var section = $"Section {ordinal}";
            var citation = new SourceCitation(documentId, chunkId, documentTitle, section, ordinal);

            chunks.Add(new DocumentChunk(
                chunkId,
                documentId,
                documentTitle,
                section,
                ordinal,
                normalized.Substring(offset, length),
                citation));

            ordinal++;
        }

        return chunks;
    }
}

