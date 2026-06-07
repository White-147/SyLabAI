namespace SyLabAI.ControlApi.Contracts;

public sealed record DocumentSummaryDto(
    Guid id,
    string title,
    string documentType,
    string status,
    string summary,
    DateTimeOffset createdAt,
    int chunkCount);

public sealed record CreateDocumentIngestionDto(
    string title,
    string documentType,
    string content,
    string? summary);

public sealed record CitationDto(
    Guid documentId,
    Guid chunkId,
    string documentTitle,
    string section,
    int chunkOrdinal);

public sealed record SearchHitDto(
    CitationDto citation,
    string snippet,
    double score);

public sealed record KnowledgeSearchDto(string query, int? limit);

public sealed record KnowledgeAnswerDto(string question, int? evidenceLimit);

public sealed record GroundedAnswerDto(
    string answer,
    IReadOnlyList<SearchHitDto> evidence,
    IReadOnlyList<string> caveats,
    bool requiresHumanReview);

public sealed record ExperimentExtractionDto(string title, string rawNote);

public sealed record StructuredExperimentRecordDto(
    Guid id,
    string title,
    IReadOnlyDictionary<string, string> conditions,
    IReadOnlyDictionary<string, string> results,
    IReadOnlyList<string> observations,
    IReadOnlyList<SearchHitDto> evidence,
    bool requiresHumanReview);

public sealed record PathSuggestionDto(
    string objective,
    string constraints);

public sealed record PathSuggestionDraftDto(
    Guid id,
    string objective,
    IReadOnlyList<string> proposedSteps,
    IReadOnlyList<string> assumptions,
    IReadOnlyList<string> risks,
    IReadOnlyList<SearchHitDto> evidence,
    bool requiresHumanReview);

public sealed record CreateLabTaskDto(
    string title,
    IReadOnlyList<string> steps,
    IReadOnlyList<string> reviewChecklist);

public sealed record LabTaskDto(
    Guid id,
    string title,
    string status,
    IReadOnlyList<string> steps,
    IReadOnlyList<string> reviewChecklist,
    DateTimeOffset createdAt);

public sealed record ProviderStatusDto(
    string provider,
    string model,
    bool configured,
    string mode,
    IReadOnlyList<string> safetyGates);

public sealed record HealthDto(
    string status,
    string service,
    string version,
    IReadOnlyList<string> runtimeDirectories,
    DateTimeOffset checkedAt);

public sealed record ValidationErrorDto(string message);

