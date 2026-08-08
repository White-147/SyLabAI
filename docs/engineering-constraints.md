# Engineering Constraints

This document is the long-form engineering boundary for SyLabAI. The root `AGENTS.md` is the short entrypoint; this file explains the durable rules in more detail.

## Priority

1. SyLabAI project constraints override generic coding preferences.
2. Generic engineering principles may strengthen maintainability, safety, and testability, but must not push the project toward Docker-first, Linux-first, local-model-first, or cloud-SaaS-first architecture.
3. More specific and newer project docs override this file when they clearly own a narrower topic.
4. Refactors must not casually change public API paths, DTO shapes, persisted schemas, default UI behavior, provider gates, security gates, or sanitizer behavior.

## Product Scope

SyLabAI is an internal lab AI assistant for Windows Server and intranet usage. Its first practical goal is to help a lab team turn scattered documents and historical experiment records into searchable, source-grounded knowledge, then use that knowledge to assist experiment path suggestions and lab task handoff.

In scope for the MVP:

- Document upload and ingestion.
- Document parsing and chunking.
- Source-grounded question answering with citations.
- Experiment record extraction and structured fields.
- Experiment path suggestion drafts with assumptions, risks, and evidence.
- Lab task cards or SOP draft handoff for manual execution.
- Result feedback and iteration records.

Out of scope for the MVP:

- Replacing lab experts or final experiment decisions.
- Autonomous chemical/material optimization without human review.
- Docker/Linux-only deployment.
- Local model hosting as a required production path.
- Public SaaS, multi-tenant billing, payment, or public internet exposure.
- Heavy agent marketplaces, plugin ecosystems, or complex visual workflow builders.

## Architecture Layers

Preferred backend shape:

```text
backend/dotnet/control-plane/src/
├── SyLabAI.ControlApi
├── SyLabAI.Application
├── SyLabAI.Domain
├── SyLabAI.Infrastructure.AI
├── SyLabAI.Infrastructure.Documents
├── SyLabAI.Infrastructure.SqlServer
└── SyLabAI.Worker
```

Layer expectations:

- `SyLabAI.ControlApi` owns HTTP composition, validation, auth/rate gates, endpoint mapping, and API module registration.
- `SyLabAI.Application` owns use cases such as document ingestion, knowledge search, experiment extraction, suggestion generation, and lab task creation.
- `SyLabAI.Domain` owns domain models, value objects, enums, and shared contracts that are independent of provider/runtime details.
- `SyLabAI.Infrastructure.AI` owns remote model provider adapters, including DeepSeek/OpenAI-compatible clients, retry policy, redaction, and provider telemetry boundaries.
- `SyLabAI.Infrastructure.Documents` owns document parser adapters, MarkItDown integration boundaries, chunking helpers, and conversion safety checks.
- `SyLabAI.Infrastructure.SqlServer` owns SQL Server persistence, schema initialization or migrations, repositories, keyword search, optional Full-Text Search integration, partition-ready table design, and optional future vector storage.
- `SyLabAI.Worker` owns background jobs that can be resumed or retried, such as ingestion, conversion, extraction, and batch suggestion runs.

Frontend shape:

```text
apps/web/src/features/
├── dashboard
├── document-library
├── knowledge-chat
├── experiment-records
├── path-suggestions
├── lab-tasks
└── settings
```

Frontend rules:

- UI calls only the Control API through API wrappers.
- UI does not read files directly from backend storage paths.
- Feature folders should own their view state, panels, forms, and feature API wrappers.
- Shared UI or client code goes to `apps/web/src/shared` only when it is genuinely reused.

## API And DTO Contracts

- Keep API routes business-named, not implementation-named.
- Public JSON keys should use camelCase.
- Do not expose local absolute paths, provider raw payloads, prompt internals, secrets, or database implementation details.
- Every AI answer that depends on documents should preserve source references when possible.
- Structured extraction should keep original source metadata so later users can audit where a field came from.

## Provider And AI Boundary

- DeepSeek API should be accessed through an OpenAI-compatible adapter, not scattered SDK calls.
- Provider configuration should include base URL, model, API key source, timeout, and cost/rate guard settings.
- Prompt construction should produce sanitized, purpose-specific prompt briefs; do not dump full internal debug prompts into logs or public DTOs.
- Live calls should return structured success, structured provider error, or structured degradation.
- Do not require embeddings in the MVP unless an embedding API is explicitly approved.
- If embeddings are later added, keep the embedding generator and vector store behind interfaces so SQL Server search can remain the baseline retrieval path.

## Document Conversion Boundary

- Treat uploaded documents as sensitive.
- Validate file type, file size, destination path, and parse mode before conversion.
- MarkItDown, Docling, or other parsers must sit behind an adapter boundary.
- Parser output should be normalized before storage: source document ID, chunk ID, page/sheet/section metadata, text content, hash, and provenance.
- Conversion failures should not leak absolute paths or raw stack traces to the UI.

## Persistence And Runtime Data

Default local folders:

```text
data/       # durable local app state and non-database runtime metadata
uploads/    # uploaded documents before/after ingestion
outputs/    # generated exports, task cards, reports
.tools/     # project-local tools
.cache/     # dependency and processing cache
.config/    # local non-secret config templates or generated settings
.tmp/       # temporary files and logs
```

Rules:

- These folders should not contain committed real user/lab data.
- The repository should include placeholders or `.gitkeep` only when needed.
- Database files, uploaded documents, generated reports, and logs should be ignored by Git.
- Scripts should infer paths from the repository root and avoid hard-coded absolute paths where possible.

## Naming

C#:

- Namespace, type, record, enum, and public member: PascalCase.
- Interface: `I` + PascalCase.
- Parameter and local variable: camelCase.
- Private readonly dependency fields: `_camelCase`.
- Async methods end with `Async`.
- File name should match the main public/internal type unless it is a clear DTO, extension, or module aggregation file.

TypeScript / React:

- Component, type, interface, class, enum: PascalCase.
- Function, variable, hook helper, API client method: lowerCamelCase.
- Hooks: `useXxx`.
- Constants: `UPPER_SNAKE_CASE`.
- CSS class names: kebab-case.

Python document-converter sidecar:

- Follow PEP 8.
- package/module/function/variable: snake_case.
- class/exception: PascalCase.
- constants: UPPER_SNAKE_CASE.
- Keep JSON boundary mapping explicit when crossing into C# DTOs.

## Verification

Before closing a change:

- Run the smallest sufficient build/test/check for the touched area.
- For docs-only changes, at least inspect the changed files and run `git diff --check` once git is initialized.
- For backend changes, run targeted `dotnet build` / `dotnet test` once the solution exists.
- For frontend changes, run `npm run lint`, `npm run test`, or `npm run build` as appropriate once the web app exists.
- For provider/document/runtime boundary changes, include a dry-run or synthetic fixture path before live data.

## Documentation Sync

Update docs when changing:

- directory layout,
- API routes or DTO contracts,
- provider configuration,
- document conversion behavior,
- persistence/runtime paths,
- security boundaries,
- setup or verification commands.
