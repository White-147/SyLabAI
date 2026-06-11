namespace SyLabAI.Application.Documents;

public sealed record DocumentConversionDryRunRequest(
    string? FileName,
    string? ContentType,
    long SizeBytes);

public sealed record DocumentConversionDryRunResult(
    bool Accepted,
    string Mode,
    string NormalizedExtension,
    long MaxSizeBytes,
    IReadOnlyList<string> SafetyChecks,
    IReadOnlyList<string> RejectionReasons);

public interface IDocumentConversionService
{
    Task<DocumentConversionDryRunResult> DryRunAsync(
        DocumentConversionDryRunRequest request,
        CancellationToken cancellationToken);
}
