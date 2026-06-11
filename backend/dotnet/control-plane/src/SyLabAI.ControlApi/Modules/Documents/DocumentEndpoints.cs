using SyLabAI.Application.Documents;
using SyLabAI.ControlApi.Contracts;

namespace SyLabAI.ControlApi.Modules.Documents;

internal static class DocumentEndpoints
{
    public static IEndpointRouteBuilder MapDocumentEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/documents").WithTags("Documents");

        group.MapGet("/", async (IDocumentIngestionService service, CancellationToken cancellationToken) =>
        {
            var documents = await service.ListDocumentsAsync(cancellationToken);
            return Results.Ok(documents.Select(document => document.ToDto()).ToArray());
        });

        group.MapPost("/ingestions", async (
            CreateDocumentIngestionDto request,
            IDocumentIngestionService service,
            CancellationToken cancellationToken) =>
        {
            var validation = Validate(request);
            if (validation is not null)
            {
                return Results.BadRequest(validation);
            }

            var document = await service.IngestAsync(
                new DocumentIngestionRequest(request.title, request.documentType, request.content, request.summary),
                cancellationToken);

            return Results.Created($"/api/documents/{document.Id}", document.ToDto());
        });

        group.MapPost("/conversions/dry-run", async (
            DocumentConversionDryRunDto request,
            IDocumentConversionService service,
            CancellationToken cancellationToken) =>
        {
            var result = await service.DryRunAsync(
                new DocumentConversionDryRunRequest(request.fileName, request.contentType, request.sizeBytes),
                cancellationToken);

            return Results.Ok(result.ToDto());
        });

        return app;
    }

    private static ValidationErrorDto? Validate(CreateDocumentIngestionDto request)
    {
        if (string.IsNullOrWhiteSpace(request.title))
        {
            return new ValidationErrorDto("Document title is required.");
        }

        if (string.IsNullOrWhiteSpace(request.documentType))
        {
            return new ValidationErrorDto("Document type is required.");
        }

        if (string.IsNullOrWhiteSpace(request.content))
        {
            return new ValidationErrorDto("Document content is required.");
        }

        if (request.content.Length > 40_000)
        {
            return new ValidationErrorDto("Demo ingestion accepts up to 40000 characters.");
        }

        return null;
    }
}
