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

## `sqlite-vec`

Path:

```text
<local-reference-root>\shaoyuan-ai-api\sqlite-vec
```

Use for:

- optional future lightweight vector search,
- Windows-friendly SQLite vector storage if embeddings are later approved.

Do not require `sqlite-vec` in the first version. Start with SQLite FTS / keyword retrieval and optional LLM reranking.

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
