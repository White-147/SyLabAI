using SyLabAI.Application.Knowledge;
using SyLabAI.Domain.Suggestions;

namespace SyLabAI.Application.Suggestions;

internal sealed class PathSuggestionService(IKnowledgeService knowledgeService) : IPathSuggestionService
{
    public async Task<PathSuggestionDraft> CreateDraftAsync(PathSuggestionRequest request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var evidence = await knowledgeService.SearchAsync(
            new KnowledgeSearchRequest($"{request.Objective} {request.Constraints}", 4),
            cancellationToken);

        var steps = new[]
        {
            "复核目标、适用材料和安全边界，确认输入资料来源。",
            "从已引用片段提取候选参数窗口，保留不确定项。",
            "设计小规模验证批次，并记录条件、偏差和观察。",
            "由实验负责人确认后再下发任务卡。"
        };

        var assumptions = new[]
        {
            "当前草案只使用已导入知识库与用户输入约束。",
            "未连接真实 DeepSeek Provider，也未使用未脱敏内部资料。"
        };

        var risks = new[]
        {
            "资料片段可能不完整，不能替代安全评审。",
            "参数迁移到新材料或新批次前需要人工验证。"
        };

        return new PathSuggestionDraft(
            Guid.NewGuid(),
            request.Objective.Trim(),
            steps,
            assumptions,
            risks,
            evidence,
            RequiresHumanReview: true);
    }
}

