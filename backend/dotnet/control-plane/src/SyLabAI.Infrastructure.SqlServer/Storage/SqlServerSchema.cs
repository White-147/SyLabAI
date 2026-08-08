using Microsoft.Data.SqlClient;

namespace SyLabAI.Infrastructure.SqlServer.Storage;

internal static class SqlServerSchema
{
    public static void EnsureCreated(SqlConnection connection)
    {
        using var command = connection.CreateCommand();
        command.CommandText = """
            IF OBJECT_ID(N'dbo.lab_documents', N'U') IS NULL
            BEGIN
                CREATE TABLE dbo.lab_documents (
                    id uniqueidentifier NOT NULL CONSTRAINT pk_lab_documents PRIMARY KEY,
                    title nvarchar(512) NOT NULL,
                    document_type nvarchar(128) NOT NULL,
                    status nvarchar(64) NOT NULL,
                    summary nvarchar(max) NOT NULL,
                    created_at datetime2(7) NOT NULL
                );
            END;

            IF OBJECT_ID(N'dbo.document_chunks', N'U') IS NULL
            BEGIN
                CREATE TABLE dbo.document_chunks (
                    id uniqueidentifier NOT NULL CONSTRAINT pk_document_chunks PRIMARY KEY,
                    document_id uniqueidentifier NOT NULL,
                    document_title nvarchar(512) NOT NULL,
                    section nvarchar(256) NOT NULL,
                    [ordinal] int NOT NULL,
                    [text] nvarchar(max) NOT NULL,
                    CONSTRAINT fk_document_chunks_lab_documents
                        FOREIGN KEY (document_id)
                        REFERENCES dbo.lab_documents(id)
                        ON DELETE CASCADE
                );
            END;

            IF OBJECT_ID(N'dbo.lab_tasks', N'U') IS NULL
            BEGIN
                CREATE TABLE dbo.lab_tasks (
                    id uniqueidentifier NOT NULL CONSTRAINT pk_lab_tasks PRIMARY KEY,
                    title nvarchar(512) NOT NULL,
                    status nvarchar(64) NOT NULL,
                    steps_json nvarchar(max) NOT NULL,
                    review_checklist_json nvarchar(max) NOT NULL,
                    created_at datetime2(7) NOT NULL
                );
            END;

            IF NOT EXISTS (
                SELECT 1
                FROM sys.indexes
                WHERE name = N'ix_document_chunks_document_id'
                    AND object_id = OBJECT_ID(N'dbo.document_chunks'))
            BEGIN
                CREATE INDEX ix_document_chunks_document_id
                    ON dbo.document_chunks(document_id);
            END;

            IF NOT EXISTS (
                SELECT 1
                FROM sys.indexes
                WHERE name = N'ix_document_chunks_document_title_ordinal'
                    AND object_id = OBJECT_ID(N'dbo.document_chunks'))
            BEGIN
                CREATE INDEX ix_document_chunks_document_title_ordinal
                    ON dbo.document_chunks(document_title, [ordinal]);
            END;

            IF NOT EXISTS (
                SELECT 1
                FROM sys.indexes
                WHERE name = N'ix_lab_tasks_created_at'
                    AND object_id = OBJECT_ID(N'dbo.lab_tasks'))
            BEGIN
                CREATE INDEX ix_lab_tasks_created_at
                    ON dbo.lab_tasks(created_at DESC);
            END;
            """;
        command.ExecuteNonQuery();
    }
}
