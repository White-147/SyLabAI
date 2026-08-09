using System.Text;
using System.Text.Json;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using SyLabAI.Application.Runtime;
using SyLabAI.Domain.Documents;
using SyLabAI.Domain.Knowledge;
using SyLabAI.Domain.Tasks;

namespace SyLabAI.Infrastructure.Sqlite.Storage;

/// <summary>
/// SQLite 版 ILabKnowledgeStore（与 SqlServerLabKnowledgeStore 同构）。
/// SQL 方言差异：无 dbo. 前缀、TOP(@limit) → LIMIT @limit、LIKE+ESCAPE → instr() 子串匹配（SQLite 不支持 ESCAPE 子句）。
/// 库文件不存在时自动创建（含目录），空表自动写入合成演示数据（SeedIfEmpty）。
/// </summary>
internal sealed class SqliteLabKnowledgeStore : ILabKnowledgeStore
{
    private readonly object _gate = new();
    private readonly string _connectionString;
    private bool _initialized;

    public SqliteLabKnowledgeStore(IConfiguration configuration)
    {
        _connectionString = SqliteConnectionStringResolver.Resolve(configuration);
    }

    public IReadOnlyList<LabDocument> GetDocuments()
    {
        lock (_gate)
        {
            using var connection = OpenConnection();
            EnsureInitialized(connection);
            return LoadDocuments(connection);
        }
    }

    public LabDocument AddDocument(LabDocument document)
    {
        lock (_gate)
        {
            using var connection = OpenConnection();
            EnsureInitialized(connection);
            using var transaction = connection.BeginTransaction();

            InsertDocument(connection, transaction, document);

            transaction.Commit();
            return document;
        }
    }

    public IReadOnlyList<DocumentChunk> GetChunks()
    {
        lock (_gate)
        {
            using var connection = OpenConnection();
            EnsureInitialized(connection);
            return LoadChunks(connection);
        }
    }

    public IReadOnlyList<DocumentChunk> SearchChunks(string query, int limit)
    {
        var trimmedQuery = query.Trim();
        if (trimmedQuery.Length == 0)
        {
            return [];
        }

        lock (_gate)
        {
            using var connection = OpenConnection();
            EnsureInitialized(connection);
            using var command = connection.CreateCommand();
            var terms = GetSearchSegments(trimmedQuery)
                .Where(term => term.Length >= 2)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Take(12)
                .ToArray();

            if (terms.Length == 0)
            {
                terms = [trimmedQuery];
            }

            // SQLite 的 LIKE 不支持 ESCAPE 子句，改用 instr() 做子串包含匹配（等价 %term%，且 %/_ 不会被当作通配符）
            var textClauses = terms
                .Select((_, index) => $"instr(\"text\", @term{index}) > 0")
                .ToArray();
            var score = terms
                .Select((_, index) => $"CASE WHEN instr(\"text\", @term{index}) > 0 THEN 1 ELSE 0 END")
                .ToArray();

            command.CommandText = $"""
                SELECT id, document_id, document_title, section, "ordinal", "text"
                FROM document_chunks
                WHERE {string.Join(" OR ", textClauses)}
                ORDER BY ({string.Join(" + ", score)}) DESC,
                         document_title,
                         "ordinal"
                LIMIT @limit;
                """;
            AddParameter(command, "limit", Math.Clamp(limit, 1, 48));

            for (var index = 0; index < terms.Length; index++)
            {
                AddParameter(command, $"term{index}", terms[index]);
            }

            using var reader = command.ExecuteReader();
            var chunks = new List<DocumentChunk>();

            while (reader.Read())
            {
                chunks.Add(ReadChunk(reader));
            }

            return chunks;
        }
    }

    public IReadOnlyList<LabTask> GetLabTasks()
    {
        lock (_gate)
        {
            using var connection = OpenConnection();
            EnsureInitialized(connection);
            using var command = connection.CreateCommand();
            command.CommandText = """
                SELECT id, title, status, steps_json, review_checklist_json, created_at
                FROM lab_tasks
                ORDER BY created_at DESC;
                """;

            using var reader = command.ExecuteReader();
            var tasks = new List<LabTask>();

            while (reader.Read())
            {
                tasks.Add(ReadTask(reader));
            }

            return tasks;
        }
    }

    public LabTask AddLabTask(LabTask task)
    {
        lock (_gate)
        {
            using var connection = OpenConnection();
            EnsureInitialized(connection);
            using var command = connection.CreateCommand();
            command.CommandText = """
                INSERT INTO lab_tasks (id, title, status, steps_json, review_checklist_json, created_at)
                VALUES (@id, @title, @status, @stepsJson, @reviewChecklistJson, @createdAt);
                """;
            AddParameter(command, "id", task.Id);
            AddParameter(command, "title", task.Title);
            AddParameter(command, "status", task.Status);
            AddParameter(command, "stepsJson", JsonSerializer.Serialize(task.Steps));
            AddParameter(command, "reviewChecklistJson", JsonSerializer.Serialize(task.ReviewChecklist));
            AddParameter(command, "createdAt", task.CreatedAt.UtcDateTime);
            command.ExecuteNonQuery();

            return task;
        }
    }

    private SqliteConnection OpenConnection()
    {
        // SQLite 文件库：自动创建父目录（Microsoft.Data.Sqlite 不会自动建目录）
        var dataSource = ExtractDataSource(_connectionString);
        if (dataSource is not null && !dataSource.StartsWith(":memory:", StringComparison.OrdinalIgnoreCase))
        {
            var directory = Path.GetDirectoryName(dataSource);
            if (!string.IsNullOrWhiteSpace(directory))
            {
                Directory.CreateDirectory(directory);
            }
        }

        var connection = new SqliteConnection(_connectionString);
        connection.Open();
        return connection;
    }

    private static string? ExtractDataSource(string connectionString)
    {
        const string prefix = "Data Source=";
        var index = connectionString.IndexOf(prefix, StringComparison.OrdinalIgnoreCase);
        if (index < 0)
        {
            return null;
        }

        var value = connectionString[(index + prefix.Length)..];
        var semicolon = value.IndexOf(';');
        return semicolon >= 0 ? value[..semicolon].Trim() : value.Trim();
    }

    private void EnsureInitialized(SqliteConnection connection)
    {
        if (_initialized)
        {
            return;
        }

        SqliteSchema.EnsureCreated(connection);
        SeedIfEmpty(connection);
        _initialized = true;
    }

    private static IReadOnlyList<LabDocument> LoadDocuments(SqliteConnection connection)
    {
        var chunks = LoadChunks(connection)
            .GroupBy(chunk => chunk.DocumentId)
            .ToDictionary(group => group.Key, group => (IReadOnlyList<DocumentChunk>)group.OrderBy(chunk => chunk.Ordinal).ToArray());

        using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT id, title, document_type, status, summary, created_at
            FROM lab_documents
            ORDER BY created_at DESC;
            """;

        using var reader = command.ExecuteReader();
        var documents = new List<LabDocument>();

        while (reader.Read())
        {
            var documentId = reader.GetGuid(0);
            documents.Add(new LabDocument(
                documentId,
                reader.GetString(1),
                reader.GetString(2),
                reader.GetString(3),
                reader.GetString(4),
                ReadDate(reader, 5),
                chunks.GetValueOrDefault(documentId, [])));
        }

        return documents;
    }

    private static IReadOnlyList<DocumentChunk> LoadChunks(SqliteConnection connection)
    {
        using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT id, document_id, document_title, section, "ordinal", "text"
            FROM document_chunks
            ORDER BY document_title, "ordinal";
            """;

        using var reader = command.ExecuteReader();
        var chunks = new List<DocumentChunk>();

        while (reader.Read())
        {
            chunks.Add(ReadChunk(reader));
        }

        return chunks;
    }

    private static DocumentChunk ReadChunk(SqliteDataReader reader)
    {
        var chunkId = reader.GetGuid(0);
        var documentId = reader.GetGuid(1);
        var documentTitle = reader.GetString(2);
        var section = reader.GetString(3);
        var ordinal = reader.GetInt32(4);
        var citation = new SourceCitation(documentId, chunkId, documentTitle, section, ordinal);

        return new DocumentChunk(
            chunkId,
            documentId,
            documentTitle,
            section,
            ordinal,
            reader.GetString(5),
            citation);
    }

    private static LabTask ReadTask(SqliteDataReader reader)
    {
        return new LabTask(
            reader.GetGuid(0),
            reader.GetString(1),
            reader.GetString(2),
            JsonSerializer.Deserialize<string[]>(reader.GetString(3)) ?? [],
            JsonSerializer.Deserialize<string[]>(reader.GetString(4)) ?? [],
            ReadDate(reader, 5));
    }

    private static void InsertDocument(SqliteConnection connection, SqliteTransaction transaction, LabDocument document)
    {
        using var documentCommand = connection.CreateCommand();
        documentCommand.Transaction = transaction;
        documentCommand.CommandText = """
            INSERT INTO lab_documents (id, title, document_type, status, summary, created_at)
            VALUES (@id, @title, @documentType, @status, @summary, @createdAt);
            """;
        AddParameter(documentCommand, "id", document.Id);
        AddParameter(documentCommand, "title", document.Title);
        AddParameter(documentCommand, "documentType", document.DocumentType);
        AddParameter(documentCommand, "status", document.Status);
        AddParameter(documentCommand, "summary", document.Summary);
        AddParameter(documentCommand, "createdAt", document.CreatedAt.UtcDateTime);
        documentCommand.ExecuteNonQuery();

        foreach (var chunk in document.Chunks)
        {
            InsertChunk(connection, transaction, chunk);
        }
    }

    private static void InsertChunk(SqliteConnection connection, SqliteTransaction transaction, DocumentChunk chunk)
    {
        using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = """
            INSERT INTO document_chunks (id, document_id, document_title, section, "ordinal", "text")
            VALUES (@id, @documentId, @documentTitle, @section, @ordinal, @text);
            """;
        AddParameter(command, "id", chunk.Id);
        AddParameter(command, "documentId", chunk.DocumentId);
        AddParameter(command, "documentTitle", chunk.DocumentTitle);
        AddParameter(command, "section", chunk.Section);
        AddParameter(command, "ordinal", chunk.Ordinal);
        AddParameter(command, "text", chunk.Text);
        command.ExecuteNonQuery();
    }

    private static void SeedIfEmpty(SqliteConnection connection)
    {
        using var countCommand = connection.CreateCommand();
        countCommand.CommandText = "SELECT COUNT(*) FROM lab_documents;";
        var count = Convert.ToInt32(countCommand.ExecuteScalar() ?? 0);

        if (count > 0)
        {
            return;
        }

        using var transaction = connection.BeginTransaction();
        foreach (var document in CreateSeedDocuments())
        {
            InsertDocument(connection, transaction, document);
        }

        InsertSeedTask(connection, transaction);
        transaction.Commit();
    }

    private static void InsertSeedTask(SqliteConnection connection, SqliteTransaction transaction)
    {
        var task = new LabTask(
            Guid.Parse("82cb4201-7562-418b-8c24-a159b01b9350"),
            "Review polymerization condition window",
            "draft",
            ["Confirm sample batch", "Check temperature and duration", "Record abnormal observations"],
            ["Source chunks attached", "Safety owner confirmed", "Result feedback planned"],
            DateTimeOffset.UtcNow.AddHours(-6));

        using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = """
            INSERT INTO lab_tasks (id, title, status, steps_json, review_checklist_json, created_at)
            VALUES (@id, @title, @status, @stepsJson, @reviewChecklistJson, @createdAt);
            """;
        AddParameter(command, "id", task.Id);
        AddParameter(command, "title", task.Title);
        AddParameter(command, "status", task.Status);
        AddParameter(command, "stepsJson", JsonSerializer.Serialize(task.Steps));
        AddParameter(command, "reviewChecklistJson", JsonSerializer.Serialize(task.ReviewChecklist));
        AddParameter(command, "createdAt", task.CreatedAt.UtcDateTime);
        command.ExecuteNonQuery();
    }

    private static IReadOnlyList<LabDocument> CreateSeedDocuments()
    {
        var firstDocumentId = Guid.Parse("17d9b96f-c6e8-4355-bdb2-5d1582cff4e0");
        var secondDocumentId = Guid.Parse("44f52cb9-d661-4d8b-bf05-9774e38afb7a");

        return
        [
            CreateDocument(
                firstDocumentId,
                "Synthetic example: waterborne resin experiment record",
                "synthetic-experiment-record",
                "A synthetic example containing temperature windows, observations, and manual review requirements.",
                [
                    "Material sample A and additive B were stirred at 65-70 C for 90 minutes. Observation: viscosity stable, no visible precipitation. Yield: 82%. This public demo text is not real lab data.",
                    "Risk note: before scale-up, manual review must confirm temperature drift, batch variance, and safety assessment. Path suggestions must preserve source citations and owner approval."
                ]),
            CreateDocument(
                secondDocumentId,
                "Synthetic example: SOP chunks and retrieval boundary",
                "synthetic-sop",
                "Demonstrates preserving provenance, section names, and chunk ordinals after document parsing.",
                [
                    "SOP draft: uploaded documents must pass file type and size validation before parsing. Conversion stores normalized text, metadata, and provenance without exposing local absolute paths to the UI.",
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

    private static void AddParameter(SqliteCommand command, string name, object value)
    {
        var parameterName = name.StartsWith('@') ? name : $"@{name}";
        command.Parameters.AddWithValue(parameterName, value);
    }

    private static IReadOnlyList<string> GetSearchSegments(string query)
    {
        var segments = new List<string>();
        var buffer = new StringBuilder();

        foreach (var character in query)
        {
            if (char.IsLetterOrDigit(character) || ContainsCjk(character))
            {
                buffer.Append(character);
                continue;
            }

            FlushBuffer();
        }

        FlushBuffer();

        var expanded = new List<string>();
        foreach (var segment in segments)
        {
            expanded.Add(segment);

            if (!segment.Any(ContainsCjk) || segment.Length <= 2)
            {
                continue;
            }

            for (var index = 0; index <= segment.Length - 2; index++)
            {
                expanded.Add(segment.Substring(index, 2));
            }
        }

        return expanded;

        void FlushBuffer()
        {
            if (buffer.Length == 0)
            {
                return;
            }

            segments.Add(buffer.ToString());
            buffer.Clear();
        }
    }

    private static bool ContainsCjk(char character)
    {
        return character is >= '\u4e00' and <= '\u9fff';
    }

    private static DateTimeOffset ReadDate(SqliteDataReader reader, int ordinal)
    {
        var dateTime = reader.GetDateTime(ordinal);
        return new DateTimeOffset(DateTime.SpecifyKind(dateTime, DateTimeKind.Utc));
    }
}
