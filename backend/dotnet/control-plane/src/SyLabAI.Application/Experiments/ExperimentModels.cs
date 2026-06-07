using SyLabAI.Domain.Experiments;

namespace SyLabAI.Application.Experiments;

public sealed record ExperimentExtractionRequest(string Title, string RawNote);

public interface IExperimentExtractionService
{
    Task<StructuredExperimentRecord> ExtractAsync(ExperimentExtractionRequest request, CancellationToken cancellationToken);
}

