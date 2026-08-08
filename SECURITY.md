# Security Policy

## Supported Scope

SyLabAI is currently an early local project skeleton. Security support applies to the current source, documentation, scripts, and configuration templates in this repository.

Historical experiments, local runtime files, raw lab documents, unredacted logs, private screenshots, and generated artifacts are outside the public support scope and must not be committed.

## Reporting Security Issues

If a security issue is discovered, do not publish exploit details, secrets, raw internal documents, local paths, or private operational evidence in public issues or comments.

Use a private security advisory channel when available. If no private channel is available, create a minimal public issue that only says there is a security concern and describes the affected area at a high level.

## Data And Secret Boundaries

Do not commit:

- DeepSeek or other provider API keys;
- tokens, cookies, certificates, private keys, or service-account files;
- raw lab documents, experiment records, supplier data, or confidential business material;
- SQL Server databases, backups, uploaded files, generated reports, parser outputs, or logs;
- raw provider payloads, prompt dumps, local absolute paths, or screenshots containing private data.

Runtime material should stay in ignored project-local folders such as `data`, `uploads`, `outputs`, `.cache`, `.config`, and `.tmp`.

## Expected Handling

Fixes should prefer least privilege, explicit configuration, input validation, output redaction, controlled file access, structured errors, and auditability.

Live provider/API calls should be explicit, guarded, and tested first with synthetic fixtures.
