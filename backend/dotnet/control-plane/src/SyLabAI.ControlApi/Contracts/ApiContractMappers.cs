using SyLabAI.Domain.Documents;
using SyLabAI.Domain.Experiments;
using SyLabAI.Domain.Knowledge;
using SyLabAI.Domain.Suggestions;
using SyLabAI.Domain.Tasks;

namespace SyLabAI.ControlApi.Contracts;

internal static class ApiContractMappers
{
    public static DocumentSummaryDto ToDto(this LabDocument document)
    {
        return new DocumentSummaryDto(
            document.Id,
            document.Title,
            document.DocumentType,
            document.Status,
            document.Summary,
            document.CreatedAt,
            document.Chunks.Count);
    }

    public static CitationDto ToDto(this SourceCitation citation)
    {
        return new CitationDto(
            citation.DocumentId,
            citation.ChunkId,
            citation.DocumentTitle,
            citation.Section,
            citation.ChunkOrdinal);
    }

    public static SearchHitDto ToDto(this SearchHit hit)
    {
        return new SearchHitDto(hit.Citation.ToDto(), hit.Snippet, hit.Score);
    }

    public static GroundedAnswerDto ToDto(this GroundedAnswer answer)
    {
        return new GroundedAnswerDto(
            answer.Answer,
            answer.Evidence.Select(ToDto).ToArray(),
            answer.Caveats,
            answer.RequiresHumanReview);
    }

    public static StructuredExperimentRecordDto ToDto(this StructuredExperimentRecord record)
    {
        return new StructuredExperimentRecordDto(
            record.Id,
            record.Title,
            record.Conditions,
            record.Results,
            record.Observations,
            record.Evidence.Select(ToDto).ToArray(),
            record.RequiresHumanReview);
    }

    public static PathSuggestionDraftDto ToDto(this PathSuggestionDraft draft)
    {
        return new PathSuggestionDraftDto(
            draft.Id,
            draft.Objective,
            draft.ProposedSteps,
            draft.Assumptions,
            draft.Risks,
            draft.Evidence.Select(ToDto).ToArray(),
            draft.RequiresHumanReview);
    }

    public static LabTaskDto ToDto(this LabTask task)
    {
        return new LabTaskDto(
            task.Id,
            task.Title,
            task.Status,
            task.Steps,
            task.ReviewChecklist,
            task.CreatedAt);
    }
}

