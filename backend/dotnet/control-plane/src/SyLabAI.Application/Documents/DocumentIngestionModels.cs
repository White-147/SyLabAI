using SyLabAI.Domain.Documents;

namespace SyLabAI.Application.Documents;

public sealed record DocumentIngestionRequest(
    string Title,
    string DocumentType,
    string Content,
    string? Summary);

public interface IDocumentIngestionService
{
    Task<IReadOnlyList<LabDocument>> ListDocumentsAsync(CancellationToken cancellationToken);

    Task<LabDocument> IngestAsync(DocumentIngestionRequest request, CancellationToken cancellationToken);
}

