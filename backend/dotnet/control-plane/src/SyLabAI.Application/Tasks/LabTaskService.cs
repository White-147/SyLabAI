using SyLabAI.Application.Runtime;
using SyLabAI.Domain.Tasks;

namespace SyLabAI.Application.Tasks;

internal sealed class LabTaskService(ILabKnowledgeStore store) : ILabTaskService
{
    public Task<IReadOnlyList<LabTask>> ListAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(store.GetLabTasks());
    }

    public Task<LabTask> CreateAsync(CreateLabTaskRequest request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var task = new LabTask(
            Guid.NewGuid(),
            request.Title.Trim(),
            "draft",
            CleanList(request.Steps),
            CleanList(request.ReviewChecklist),
            DateTimeOffset.UtcNow);

        return Task.FromResult(store.AddLabTask(task));
    }

    private static IReadOnlyList<string> CleanList(IReadOnlyList<string> values)
    {
        var cleaned = values
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Select(value => value.Trim())
            .ToArray();

        return cleaned.Length == 0 ? ["待补充"] : cleaned;
    }
}

