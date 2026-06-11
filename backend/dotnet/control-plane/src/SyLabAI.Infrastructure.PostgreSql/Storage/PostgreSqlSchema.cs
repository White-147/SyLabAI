using Npgsql;

namespace SyLabAI.Infrastructure.PostgreSql.Storage;

internal static class PostgreSqlSchema
{
    public static void EnsureCreated(NpgsqlConnection connection)
    {
        using var command = connection.CreateCommand();
        command.CommandText = """
            CREATE TABLE IF NOT EXISTS lab_documents (
                id uuid PRIMARY KEY,
                title text NOT NULL,
                document_type text NOT NULL,
                status text NOT NULL,
                summary text NOT NULL,
                created_at timestamptz NOT NULL
            );

            CREATE TABLE IF NOT EXISTS document_chunks (
                id uuid PRIMARY KEY,
                document_id uuid NOT NULL REFERENCES lab_documents(id) ON DELETE CASCADE,
                document_title text NOT NULL,
                section text NOT NULL,
                ordinal integer NOT NULL,
                text text NOT NULL
            );

            CREATE TABLE IF NOT EXISTS lab_tasks (
                id uuid PRIMARY KEY,
                title text NOT NULL,
                status text NOT NULL,
                steps_json text NOT NULL,
                review_checklist_json text NOT NULL,
                created_at timestamptz NOT NULL
            );

            CREATE INDEX IF NOT EXISTS ix_document_chunks_document_id
                ON document_chunks(document_id);

            CREATE INDEX IF NOT EXISTS ix_document_chunks_search
                ON document_chunks USING gin (to_tsvector('simple', text));

            CREATE INDEX IF NOT EXISTS ix_lab_tasks_created_at
                ON lab_tasks(created_at DESC);
            """;
        command.ExecuteNonQuery();
    }
}
