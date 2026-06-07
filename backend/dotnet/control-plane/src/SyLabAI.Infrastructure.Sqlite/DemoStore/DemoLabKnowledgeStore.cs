using SyLabAI.Application.Runtime;
using SyLabAI.Domain.Documents;
using SyLabAI.Domain.Knowledge;
using SyLabAI.Domain.Tasks;

namespace SyLabAI.Infrastructure.Sqlite.DemoStore;

internal sealed class DemoLabKnowledgeStore : ILabKnowledgeStore
{
    private readonly object _gate = new();
    private readonly List<LabDocument> _documents;
    private readonly List<LabTask> _tasks;

    public DemoLabKnowledgeStore()
    {
        _documents = CreateSeedDocuments();
        _tasks =
        [
            new LabTask(
                Guid.Parse("82cb4201-7562-418b-8c24-a159b01b9350"),
                "复核聚合条件窗口",
                "draft",
                ["确认样品批次", "核对温度和时长", "记录异常现象"],
                ["来源片段已附带", "安全负责人已确认", "结果反馈已安排"],
                DateTimeOffset.UtcNow.AddHours(-6))
        ];
    }

    public IReadOnlyList<LabDocument> GetDocuments()
    {
        lock (_gate)
        {
            return _documents.ToArray();
        }
    }

    public LabDocument AddDocument(LabDocument document)
    {
        lock (_gate)
        {
            _documents.Insert(0, document);
            return document;
        }
    }

    public IReadOnlyList<DocumentChunk> GetChunks()
    {
        lock (_gate)
        {
            return _documents.SelectMany(document => document.Chunks).ToArray();
        }
    }

    public IReadOnlyList<LabTask> GetLabTasks()
    {
        lock (_gate)
        {
            return _tasks.ToArray();
        }
    }

    public LabTask AddLabTask(LabTask task)
    {
        lock (_gate)
        {
            _tasks.Insert(0, task);
            return task;
        }
    }

    private static List<LabDocument> CreateSeedDocuments()
    {
        var firstDocumentId = Guid.Parse("17d9b96f-c6e8-4355-bdb2-5d1582cff4e0");
        var secondDocumentId = Guid.Parse("44f52cb9-d661-4d8b-bf05-9774e38afb7a");

        return
        [
            CreateDocument(
                firstDocumentId,
                "合成样例：水性树脂实验记录",
                "synthetic-experiment-record",
                "包含温度窗口、观察记录和人工复核要求的合成样例。",
                [
                    "材料 sample A 与助剂 B 在 65-70 C 温度窗口下搅拌 90 min。observation: 黏度稳定，未见明显沉降。yield: 82%。该记录仅用于公开 Demo，不代表真实实验数据。",
                    "风险记录：放大前需人工复核温度漂移、批次差异和安全评审。路径建议必须保留来源引用并由实验负责人确认。"
                ]),
            CreateDocument(
                secondDocumentId,
                "合成样例：SOP 片段与检索边界",
                "synthetic-sop",
                "演示文档解析后保留来源、章节和 chunk 编号。",
                [
                    "SOP draft: 上传文档先进行文件类型和大小校验，再进入解析边界。转换结果只保存 normalized text, metadata, provenance，不向 UI 暴露本地绝对路径。",
                    "Knowledge answer must be source-grounded with citations. AI output is advisory and must not replace lab safety review, chemical judgment, or final experiment decisions."
                ])
        ];
    }

    private static LabDocument CreateDocument(
        Guid documentId,
        string title,
        string documentType,
        string summary,
        IReadOnlyList<string> chunkTexts)
    {
        var chunks = chunkTexts
            .Select((text, index) =>
            {
                var chunkId = Guid.NewGuid();
                var section = $"Seed {index + 1}";
                var citation = new SourceCitation(documentId, chunkId, title, section, index + 1);

                return new DocumentChunk(
                    chunkId,
                    documentId,
                    title,
                    section,
                    index + 1,
                    text,
                    citation);
            })
            .ToArray();

        return new LabDocument(
            documentId,
            title,
            documentType,
            "parsed",
            summary,
            DateTimeOffset.UtcNow.AddDays(-1),
            chunks);
    }
}
