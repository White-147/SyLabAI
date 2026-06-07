using SyLabAI.Application.Knowledge;
using SyLabAI.Domain.Experiments;

namespace SyLabAI.Application.Experiments;

internal sealed class ExperimentExtractionService(IKnowledgeService knowledgeService) : IExperimentExtractionService
{
    public async Task<StructuredExperimentRecord> ExtractAsync(ExperimentExtractionRequest request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var evidence = await knowledgeService.SearchAsync(
            new KnowledgeSearchRequest(request.RawNote, 3),
            cancellationToken);

        var note = request.RawNote;
        var conditions = new Dictionary<string, string>
        {
            ["temperature"] = FindValue(note, ["temperature", "temp", "温度"]) ?? "待人工补充",
            ["time"] = FindValue(note, ["time", "duration", "时间", "时长"]) ?? "待人工补充",
            ["material"] = FindValue(note, ["material", "sample", "材料", "样品"]) ?? "待人工补充"
        };

        var results = new Dictionary<string, string>
        {
            ["yield"] = FindValue(note, ["yield", "收率"]) ?? "未记录",
            ["observation"] = FindValue(note, ["observation", "observed", "现象", "观察"]) ?? "未记录"
        };

        var observations = note
            .Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries)
            .Select(line => line.Trim())
            .Where(line => line.Length > 0)
            .Take(4)
            .ToArray();

        return new StructuredExperimentRecord(
            Guid.NewGuid(),
            request.Title.Trim(),
            conditions,
            results,
            observations,
            evidence,
            RequiresHumanReview: true);
    }

    private static string? FindValue(string text, string[] labels)
    {
        foreach (var line in text.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries))
        {
            foreach (var label in labels)
            {
                var index = line.IndexOf(label, StringComparison.OrdinalIgnoreCase);
                if (index < 0)
                {
                    continue;
                }

                var valueStart = index + label.Length;
                var value = line[valueStart..].Trim(' ', ':', '：', '-', '=');
                if (value.Length > 0)
                {
                    return value.Length <= 80 ? value : value[..80] + "...";
                }
            }
        }

        return null;
    }
}

