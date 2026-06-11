# SyLabAI Engineering Constraints

These instructions apply to the SyLabAI repository. Read this file before making non-trivial code, configuration, script, or documentation changes.

## Product Boundary

- SyLabAI is a Windows Server / intranet-first lab AI assistant for experiment documents, experiment records, knowledge search, path suggestions, lab task handoff, and result feedback.
- The first version uses remote model APIs only. Do not make local model hosting, Ollama, Docker, Linux, Kubernetes, Redis, or cloud SaaS a production prerequisite.
- AI output is advisory. The system must not claim to replace lab specialists, chemical/material judgment, safety review, or final experiment decisions.
- Keep the MVP focused: document ingestion, source-grounded Q&A, experiment record structuring, path suggestions, lab task cards, and feedback loops.

## Architecture Boundary

- Frontend code must access backend capabilities only through SyLabAI Control API DTOs and API wrappers.
- UI code must not directly access PostgreSQL, local files, API keys, provider SDKs, Python scripts, document converters, or runtime caches.
- Provider calls, document conversion, retrieval, storage, and file system access must live behind explicit adapters, services, or IO boundaries.
- Do not put business logic directly into HTTP endpoints. Endpoints should validate/route/compose; application services own use cases.
- Keep public DTO shapes, API routes, persisted schemas, auth gates, sanitizer behavior, and default UI workflows stable unless the task explicitly changes them.

## Windows And Filesystem Boundary

- The repository must remain usable from a non-C drive path such as `<non-c-drive>\code\SyLabAI`.
- Runtime data, logs, uploads, generated outputs, caches, temporary files, and project-local tool state should stay under this repository.
- Use project-local folders by default: `.tools`, `.cache`, `.config`, `.tmp`, `data`, `uploads`, and `outputs`.
- Do not write dependencies, caches, runtime data, generated assets, or reusable tool state to C drive user directories, system directories, global user directories, or machine-wide PATH unless the user explicitly approves it.
- If PATH or environment variables are involved, distinguish Machine/System PATH, User PATH, and current-process PATH before changing anything.

## Security And Privacy

- Never commit API keys, provider secrets, service accounts, certificates, database files, raw experiment documents, internal lab data, supplier data, logs, screenshots containing private data, raw provider payloads, or local absolute paths.
- Public API responses, logs, run records, and screenshots must not expose provider secrets, local absolute paths, consumable file paths, raw command lines, or sensitive document contents.
- File access should go through controlled metadata, preview, download, or artifact endpoints; do not hand local paths to the frontend.
- Live provider calls require explicit configuration, spend/rate guard, sanitized prompts, structured request records, and clear user/developer intent.
- Document conversion must validate file type, file size, destination path, and trust boundary before processing.

## Engineering Discipline

- Prefer high cohesion, low coupling, single responsibility, separation of concerns, and dependency inversion.
- Follow existing local patterns once the project has them. Before then, keep abstractions small and purpose-named.
- Do not add broad `Helpers`, `Utils`, catch-all services, or vague abstraction layers.
- Add an abstraction only when it removes real complexity, isolates a real variation point, reduces meaningful duplication, or matches an established local pattern.
- Temporary MVP shortcuts are acceptable only when contained in one clearly named adapter/gateway and recorded as temporary debt in docs.
- Work with existing user changes; do not revert changes you did not make.

## Technology Direction

- Backend: ASP.NET Core Web API under `backend/dotnet/control-plane`.
- Frontend: React + TypeScript under `apps/web`.
- Storage: PostgreSQL for MVP and later deployment. Do not keep SQLite as a parallel runtime database.
- Model calls: DeepSeek API through an OpenAI-compatible provider adapter.
- Retrieval: start with PostgreSQL full-text search / keyword retrieval and optional LLM reranking. Do not require embeddings until an approved embedding API exists.
- Document conversion: keep MarkItDown or any parser behind `backend/services/document-converter` or a backend adapter boundary.

## Documentation And Verification

- Keep long-lived docs in `docs/`. The root README should be a concise product and engineering entry.
- Update relevant docs when changing architecture, runtime behavior, API contracts, security boundaries, or setup commands.
- Run the smallest sufficient verification for the touched surface and report what passed or could not be run.
- For backend contract changes, run targeted .NET tests once the test project exists.
- For frontend changes, run lint/build/tests once the frontend scaffold exists.
- Always run `git diff --check` before committing once the repository is initialized.
