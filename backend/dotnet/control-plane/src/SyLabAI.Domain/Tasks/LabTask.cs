namespace SyLabAI.Domain.Tasks;

public sealed record LabTask(
    Guid Id,
    string Title,
    string Status,
    IReadOnlyList<string> Steps,
    IReadOnlyList<string> ReviewChecklist,
    DateTimeOffset CreatedAt);

