using SyLabAI.Domain.Tasks;

namespace SyLabAI.Application.Tasks;

public sealed record CreateLabTaskRequest(
    string Title,
    IReadOnlyList<string> Steps,
    IReadOnlyList<string> ReviewChecklist);

public interface ILabTaskService
{
    Task<IReadOnlyList<LabTask>> ListAsync(CancellationToken cancellationToken);

    Task<LabTask> CreateAsync(CreateLabTaskRequest request, CancellationToken cancellationToken);
}

