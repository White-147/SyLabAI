# Operations And Evidence Boundary

This document defines what must not be committed and how operational evidence should be handled.

## Do Not Commit

The following material must not enter Git:

- DeepSeek or other provider API keys.
- Service accounts, certificates, private keys, tokens, cookies, or credentials.
- Raw internal lab documents, experiment records, supplier data, customer data, or business confidential files.
- SQL Server databases, backups, exported tables, or future analytical-store backups.
- Uploaded files in `uploads/`.
- Generated task cards, reports, exports, and outputs in `outputs/`.
- Runtime logs, temporary files, parser cache, model/API raw payloads, or local service state.
- Screenshots or run records that expose private documents, local absolute paths, API keys, or raw prompts.

## Allowed In Git

The repository may contain:

- source code,
- documentation,
- sanitized examples,
- synthetic fixtures,
- config templates without real secrets,
- verification scripts,
- public-safe architecture diagrams.

## Runtime Directories

Default local runtime folders:

```text
data/
uploads/
outputs/
.tools/
.cache/
.config/
.tmp/
```

These directories may exist in the working tree, but their real contents should be ignored. Use placeholders only when a directory must be preserved.

## Evidence Rules

Validation evidence should be synthetic by default.

If real operational evidence is ever needed:

- store it outside Git,
- redact secrets and private document content,
- avoid local absolute paths in shared output,
- summarize the result in docs instead of committing raw logs,
- keep a clear distinction between synthetic fixtures and real company data.

## Provider Calls

Live provider/API calls require explicit intent and safe configuration.

Before live calls:

- confirm which provider and model will be used,
- confirm the API key source,
- use sanitized prompts,
- avoid raw internal document dumps,
- record only structured, redacted results,
- preserve source citations and human review state.

## Document Handling

Uploaded documents should be treated as private by default.

Document processing should:

- validate file type and size,
- keep parsing inside project-local runtime folders,
- store normalized chunks and metadata rather than uncontrolled parser output,
- avoid exposing raw local paths to the UI,
- return structured errors on parser failure.
