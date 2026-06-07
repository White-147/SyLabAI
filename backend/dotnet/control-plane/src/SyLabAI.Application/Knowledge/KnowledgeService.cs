using SyLabAI.Application.Runtime;
using SyLabAI.Domain.Documents;
using SyLabAI.Domain.Knowledge;

namespace SyLabAI.Application.Knowledge;

internal sealed class KnowledgeService(ILabKnowledgeStore store) : IKnowledgeService
{
    public Task<IReadOnlyList<SearchHit>> SearchAsync(KnowledgeSearchRequest request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var terms = GetSearchTerms(request.Query);
        var hits = store.GetChunks()
            .Select(chunk => ScoreChunk(chunk, request.Query, terms))
            .Where(hit => hit.Score > 0)
            .OrderByDescending(hit => hit.Score)
            .ThenBy(hit => hit.Citation.DocumentTitle)
            .Take(Math.Clamp(request.Limit, 1, 12))
            .ToArray();

        return Task.FromResult<IReadOnlyList<SearchHit>>(hits);
    }

    public async Task<GroundedAnswer> AnswerAsync(KnowledgeAnswerRequest request, CancellationToken cancellationToken)
    {
        var evidence = await SearchAsync(
            new KnowledgeSearchRequest(request.Question, request.EvidenceLimit),
            cancellationToken);

        if (evidence.Count == 0)
        {
            return new GroundedAnswer(
                "当前知识库没有找到足够的来源片段。请先导入相关实验资料，或换一个更具体的问题。",
                evidence,
                ["回答必须由来源片段支撑。", "实验判断仍需人工复核。"],
                RequiresHumanReview: true);
        }

        var lead = evidence[0];
        var answer = $"基于已索引资料，问题“{request.Question.Trim()}”的可用依据主要来自《{lead.Citation.DocumentTitle}》的“{lead.Citation.Section}”。当前回答只能作为检索摘要：请结合引用片段复核实验条件、风险和适用范围。";

        return new GroundedAnswer(
            answer,
            evidence,
            ["这是来源摘要，不是最终实验结论。", "涉及安全、材料或工艺决策时必须由实验负责人确认。"],
            RequiresHumanReview: true);
    }

    private static SearchHit ScoreChunk(DocumentChunk chunk, string rawQuery, IReadOnlyList<string> terms)
    {
        var text = chunk.Text;
        var score = 0d;

        foreach (var term in terms)
        {
            if (text.Contains(term, StringComparison.OrdinalIgnoreCase))
            {
                score += term.Length >= 4 ? 2.0 : 1.0;
            }
        }

        var trimmedQuery = rawQuery.Trim();
        if (trimmedQuery.Length > 0 && text.Contains(trimmedQuery, StringComparison.OrdinalIgnoreCase))
        {
            score += 3.0;
        }

        return new SearchHit(chunk.Citation, BuildSnippet(text, terms), score);
    }

    private static IReadOnlyList<string> GetSearchTerms(string query)
    {
        var terms = new List<string>();
        var segments = query
            .Split([' ', '\t', '\r', '\n', ',', '.', ';', ':', '，', '。', '；', '：'], StringSplitOptions.RemoveEmptyEntries)
            .Where(term => term.Length >= 2)
            .ToArray();

        foreach (var segment in segments)
        {
            terms.Add(segment);

            if (!ContainsCjk(segment) || segment.Length <= 2)
            {
                continue;
            }

            for (var index = 0; index <= segment.Length - 2; index++)
            {
                terms.Add(segment.Substring(index, 2));
            }
        }

        var distinctTerms = terms
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        return distinctTerms.Length == 0 && query.Trim().Length > 0 ? [query.Trim()] : distinctTerms;
    }

    private static bool ContainsCjk(string value)
    {
        return value.Any(character => character is >= '\u4e00' and <= '\u9fff');
    }

    private static string BuildSnippet(string text, IReadOnlyList<string> terms)
    {
        var firstTerm = terms.FirstOrDefault(term => text.Contains(term, StringComparison.OrdinalIgnoreCase));
        var start = firstTerm is null
            ? 0
            : Math.Max(0, text.IndexOf(firstTerm, StringComparison.OrdinalIgnoreCase) - 48);
        var length = Math.Min(180, text.Length - start);
        var snippet = text.Substring(start, length).Trim();

        return start > 0 ? "..." + snippet : snippet;
    }
}
