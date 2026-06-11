using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Npgsql;
using SyLabAI.Application.Runtime;
using SyLabAI.Domain.Documents;
using SyLabAI.Domain.Knowledge;
using SyLabAI.Domain.Tasks;

namespace SyLabAI.Infrastructure.PostgreSql.Storage;

internal sealed class PostgreSqlLabKnowledgeStore : ILabKnowledgeStore
{
    private readonly object _gate = new();
    private readonly string? _connectionString;
    private bool _initialized;

    public PostgreSqlLabKnowledgeStore(IConfiguration configuration)
    {
        _connectionString = PostgreSqlConnectionStringResolver.Resolve(configuration);
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

            var textClauses = terms
                .Select((_, index) => $"text ILIKE @term{index}")
                .ToArray();
            var where = textClauses.Length == 0
                ? "to_tsvector('simple', text) @@ websearch_to_tsquery('simple', @query)"
                : $"to_tsvector('simple', text) @@ websearch_to_tsquery('simple', @query) OR {string.Join(" OR ", textClauses)}";

            command.CommandText = $"""
                SELECT id, document_id, document_title, section, ordinal, text
                FROM document_chunks
                WHERE {where}
                ORDER BY ts_rank_cd(to_tsvector('simple', text), websearch_to_tsquery('simple', @query)) DESC,
                         document_title,
                         ordinal
                LIMIT @limit;
                """;
            AddParameter(command, "query", trimmedQuery);
            AddParameter(command, "limit", Math.Clamp(limit, 1, 48));

            for (var index = 0; index < terms.Length; index++)
            {
                AddParameter(command, $"term{index}", $"%{terms[index]}%");
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

    private NpgsqlConnection OpenConnection()
    {
        var connectionString = _connectionString;
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "PostgreSQL connection string is not configured. Set ConnectionStrings:SyLabAI, SyLabAI:PostgreSql:ConnectionString, or SYLABAI_POSTGRES_CONNECTION_STRING.");
        }

        var connection = new NpgsqlConnection(connectionString);
        connection.Open();
        return connection;
    }

    private void EnsureInitialized(NpgsqlConnection connection)
    {
        if (_initialized)
        {
            return;
        }

        PostgreSqlSchema.EnsureCreated(connection);
        SeedIfEmpty(connection);
        _initialized = true;
    }

    private static IReadOnlyList<LabDocument> LoadDocuments(NpgsqlConnection connection)
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

    private static IReadOnlyList<DocumentChunk> LoadChunks(NpgsqlConnection connection)
    {
        using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT id, document_id, document_title, section, ordinal, text
            FROM document_chunks
            ORDER BY document_title, ordinal;
            """;

        using var reader = command.ExecuteReader();
        var chunks = new List<DocumentChunk>();

        while (reader.Read())
        {
            chunks.Add(ReadChunk(reader));
        }

        return chunks;
    }

    private static DocumentChunk ReadChunk(NpgsqlDataReader reader)
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

    private static LabTask ReadTask(NpgsqlDataReader reader)
    {
        return new LabTask(
            reader.GetGuid(0),
            reader.GetString(1),
            reader.GetString(2),
            JsonSerializer.Deserialize<string[]>(reader.GetString(3)) ?? [],
            JsonSerializer.Deserialize<string[]>(reader.GetString(4)) ?? [],
            ReadDate(reader, 5));
    }

    private static void InsertDocument(NpgsqlConnection connection, NpgsqlTransaction transaction, LabDocument document)
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

    private static void InsertChunk(NpgsqlConnection connection, NpgsqlTransaction transaction, DocumentChunk chunk)
    {
        using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = """
            INSERT INTO document_chunks (id, document_id, document_title, section, ordinal, text)
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

    private static void SeedIfEmpty(NpgsqlConnection connection)
    {
        using var countCommand = connection.CreateCommand();
        countCommand.CommandText = "SELECT COUNT(*) FROM lab_documents;";
        var count = (long)(countCommand.ExecuteScalar() ?? 0L);

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

    private static void InsertSeedTask(NpgsqlConnection connection, NpgsqlTransaction transaction)
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

    private static void AddParameter(NpgsqlCommand command, string name, object value)
    {
        command.Parameters.AddWithValue(name, value);
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

    private static DateTimeOffset ReadDate(NpgsqlDataReader reader, int ordinal)
    {
        var dateTime = reader.GetDateTime(ordinal);
        return new DateTimeOffset(DateTime.SpecifyKind(dateTime, DateTimeKind.Utc));
    }
}
