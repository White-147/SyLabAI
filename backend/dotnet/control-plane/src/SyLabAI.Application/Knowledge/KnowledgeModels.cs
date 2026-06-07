using SyLabAI.Domain.Knowledge;

namespace SyLabAI.Application.Knowledge;

public sealed record KnowledgeSearchRequest(string Query, int Limit = 5);

public sealed record KnowledgeAnswerRequest(string Question, int EvidenceLimit = 4);

public interface IKnowledgeService
{
    Task<IReadOnlyList<SearchHit>> SearchAsync(KnowledgeSearchRequest request, CancellationToken cancellationToken);

    Task<GroundedAnswer> AnswerAsync(KnowledgeAnswerRequest request, CancellationToken cancellationToken);
}

