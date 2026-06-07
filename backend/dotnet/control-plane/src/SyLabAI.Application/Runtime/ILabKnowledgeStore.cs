using SyLabAI.Domain.Documents;
using SyLabAI.Domain.Tasks;

namespace SyLabAI.Application.Runtime;

public interface ILabKnowledgeStore
{
    IReadOnlyList<LabDocument> GetDocuments();

    LabDocument AddDocument(LabDocument document);

    IReadOnlyList<DocumentChunk> GetChunks();

    IReadOnlyList<LabTask> GetLabTasks();

    LabTask AddLabTask(LabTask task);
}

