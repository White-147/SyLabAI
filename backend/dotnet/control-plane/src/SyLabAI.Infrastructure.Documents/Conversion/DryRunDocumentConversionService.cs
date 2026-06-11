using SyLabAI.Application.Documents;

namespace SyLabAI.Infrastructure.Documents.Conversion;

internal sealed class DryRunDocumentConversionService : IDocumentConversionService
{
    private const long MaxSizeBytes = 25 * 1024 * 1024;

    private static readonly string[] SupportedExtensions =
    [
        ".txt",
        ".md",
        ".pdf",
        ".docx",
        ".xlsx",
        ".pptx",
        ".csv",
        ".json",
        ".html"
    ];

    private static readonly string[] SafetyChecks =
    [
        "File type is validated before parser handoff.",
        "Conversion output is normalized text and metadata only.",
        "Runtime paths stay behind backend adapters.",
        "Local absolute paths are not returned to the frontend.",
        "AI or parser output remains advisory and requires human review."
    ];

    public Task<DocumentConversionDryRunResult> DryRunAsync(
        DocumentConversionDryRunRequest request,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var extension = NormalizeExtension(request.FileName);
        var reasons = new List<string>();

        if (string.IsNullOrWhiteSpace(request.FileName))
        {
            reasons.Add("File name is required.");
        }

        if (extension.Length == 0 || !SupportedExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase))
        {
            reasons.Add("File extension is not enabled for the MVP converter boundary.");
        }

        if (request.SizeBytes <= 0)
        {
            reasons.Add("File size must be greater than zero.");
        }
        else if (request.SizeBytes > MaxSizeBytes)
        {
            reasons.Add($"File size exceeds the dry-run limit of {MaxSizeBytes} bytes.");
        }

        var result = new DocumentConversionDryRunResult(
            reasons.Count == 0,
            "dry-run",
            extension,
            MaxSizeBytes,
            SafetyChecks,
            reasons);

        return Task.FromResult(result);
    }

    private static string NormalizeExtension(string? fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
        {
            return string.Empty;
        }

        return Path.GetExtension(fileName.Trim()).ToLowerInvariant();
    }
}
