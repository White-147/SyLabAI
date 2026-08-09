using Microsoft.Data.Sqlite;

namespace SyLabAI.Infrastructure.Sqlite.Storage;

/// <summary>
/// SQLite 建表（幂等）：与 SqlServerSchema 同构，库文件不存在时自动创建。
/// </summary>
internal static class SqliteSchema
{
    public static void EnsureCreated(SqliteConnection connection)
    {
        using var command = connection.CreateCommand();
        command.CommandText = """
            CREATE TABLE IF NOT EXISTS lab_documents (
                id            TEXT PRIMARY KEY,
                title         TEXT NOT NULL,
                document_type TEXT NOT NULL,
                status        TEXT NOT NULL,
                summary       TEXT NOT NULL,
                created_at    TEXT NOT NULL
            );
            CREATE TABLE IF NOT EXISTS document_chunks (
                id             TEXT PRIMARY KEY,
                document_id    TEXT NOT NULL,
                document_title TEXT NOT NULL,
                section        TEXT NOT NULL,
                ordinal        INTEGER NOT NULL,
                text           TEXT NOT NULL
            );
            CREATE TABLE IF NOT EXISTS lab_tasks (
                id                   TEXT PRIMARY KEY,
                title                TEXT NOT NULL,
                status               TEXT NOT NULL,
                steps_json           TEXT NOT NULL,
                review_checklist_json TEXT NOT NULL,
                created_at           TEXT NOT NULL
            );
            CREATE INDEX IF NOT EXISTS idx_chunks_document_id ON document_chunks(document_id);
            """;
        command.ExecuteNonQuery();
    }
}
