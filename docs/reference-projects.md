# Reference Projects

SyLabAI uses selected ideas from local and external reference projects. These projects are references, not upstream bases to copy wholesale.

The reference projects were cloned in a local research workspace outside this repository. They are references only and should not be vendored into SyLabAI.

```text
<local-reference-root>\shaoyuan-ai-api
```

## `dotnet-ai-samples`

Path:

```text
<local-reference-root>\shaoyuan-ai-api\dotnet-ai-samples
```

Use for:

- `Microsoft.Extensions.AI` patterns.
- OpenAI-compatible client examples.
- Streaming, tool calling, caching, dependency injection, and provider abstractions.

Relevant paths:

```text
src\microsoft-extensions-ai\openai\OpenAIWebAPI
src\microsoft-extensions-ai\openai\OpenAIExamples
src\microsoft-extensions-ai\abstraction-implementations
```

SyLabAI should borrow the abstraction style, not the exact OpenAI-only configuration. DeepSeek should live behind a provider adapter with configurable base URL, model, timeout, API key source, and structured errors.

Current adoption:

- Provider status is exposed through an application interface and an infrastructure implementation.
- DeepSeek configuration is centralized under `SyLabAI:Provider`.
- API keys are detected by source only and are not returned through DTOs.
- Live calls remain explicitly gated while the first provider adapter is wired.

## `agent-framework-samples`

Path:

```text
<local-reference-root>\shaoyuan-ai-api\agent-framework-samples
```

Use for:

- later workflow ideas,
- RAG concepts,
- human-in-the-loop and multi-step orchestration references.

Relevant paths:

```text
06.RAGs
07.Workflow
08.EvaluationAndTracing
```

Do not make Microsoft Foundry or Agent Framework a hard dependency for the MVP. Treat these samples as future workflow references.

## `markitdown`

Path:

```text
<local-reference-root>\shaoyuan-ai-api\markitdown
```

Use for:

- document-to-Markdown/text conversion,
- PDF, Word, Excel, PowerPoint, HTML, CSV, JSON, XML, ZIP and similar file ingestion.

SyLabAI should keep MarkItDown behind a document converter boundary and sanitize file access. The converter should return normalized text and metadata, not write directly to the app database.

Current adoption:

- `POST /api/documents/conversions/dry-run` validates file metadata before any parser handoff.
- The dry-run adapter returns safety checks and rejection reasons without reading files or writing uploads.
- Text ingestion remains separate from future parser execution.

## `sqlite-vec`

Path:

```text
<local-reference-root>\shaoyuan-ai-api\sqlite-vec
```

Historical reference only:

- optional future lightweight vector search ideas,
- embedded-database tradeoff comparison.

Do not use SQLite or `sqlite-vec` as a runtime database in SyLabAI. The project uses SQL Server as its single database direction.

Current adoption:

- No SQLite runtime storage is retained.
- Knowledge search asks the SQL Server store for candidates first, then applies application-layer scoring and fallback.
- No vector dependency is required for the MVP.

## `SQL Server`

Use for:

- production-ready relational storage,
- large document and crawler-derived datasets,
- partition-ready schema design,
- keyword retrieval, optional Full-Text Search, and indexed metadata lookup,
- future vector extensions only if embeddings are approved.

Current adoption:

- `SyLabAI.Infrastructure.SqlServer` is the single persistence infrastructure project.
- Runtime configuration uses `ConnectionStrings:SyLabAI`, `SyLabAI:SqlServer:ConnectionString`, or `SYLABAI_SQLSERVER_CONNECTION_STRING`.
- SQLite is not kept as a fallback.

## Local Project Lessons

MiLuStudio contributes useful constraints:

- Windows-local product boundary,
- Control API / DTO boundary,
- provider and runtime adapters,
- project-local writable state,
- no secrets or local paths in public DTOs/logs.

XiaoLouAI contributes useful structure:

- concise docs under `docs/`,
- frontend grouped by product area,
- backend grouped by Control API modules and infrastructure boundaries,
- operation/evidence separation.

SyLabAI should remain lighter than both.
