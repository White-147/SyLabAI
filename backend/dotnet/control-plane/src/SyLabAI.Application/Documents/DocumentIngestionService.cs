using SyLabAI.Application.Runtime;
using SyLabAI.Domain.Documents;

namespace SyLabAI.Application.Documents;

internal sealed class DocumentIngestionService(
    ILabKnowledgeStore store,
    IDocumentChunker chunker) : IDocumentIngestionService
{
    public Task<IReadOnlyList<LabDocument>> ListDocumentsAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(store.GetDocuments());
    }

    public Task<LabDocument> IngestAsync(DocumentIngestionRequest request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var documentId = Guid.NewGuid();
        var chunks = chunker.Chunk(documentId, request.Title.Trim(), request.Content);
        var summary = string.IsNullOrWhiteSpace(request.Summary)
            ? BuildSummary(request.Content)
            : request.Summary.Trim();

        var document = new LabDocument(
            documentId,
            request.Title.Trim(),
            request.DocumentType.Trim(),
            "parsed",
            summary,
            DateTimeOffset.UtcNow,
            chunks);

        return Task.FromResult(store.AddDocument(document));
    }

    private static string BuildSummary(string content)
    {
        var normalized = string.Join(' ', content.Split(default(char[]), StringSplitOptions.RemoveEmptyEntries));
        return normalized.Length <= 140 ? normalized : normalized[..140] + "...";
    }
}

