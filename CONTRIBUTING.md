# Contributing

SyLabAI is currently a small internal project skeleton. Contributions should keep the Windows Server / intranet-first, API-only boundary intact.

## Before Starting

Read:

```powershell
Get-Content .\AGENTS.md -Encoding UTF8
Get-Content .\docs\engineering-constraints.md -Encoding UTF8
Get-Content .\docs\architecture.md -Encoding UTF8
```

## Engineering Expectations

- Keep changes scoped to one clear reason.
- Preserve API routes, DTO shapes, persisted schemas, security gates, and runtime path boundaries unless the task explicitly changes them.
- Do not introduce Docker/Linux/local-model requirements for the MVP.
- Do not commit secrets, real lab data, raw logs, database files, generated outputs, or private screenshots.
- Keep provider, document conversion, file system, retrieval, and persistence details behind explicit boundaries.
- Avoid broad `Helpers`, `Utils`, or catch-all services.

## Verification

Run the smallest sufficient checks for the touched area.

Once scaffolded, expected checks include:

```powershell
dotnet build .\backend\dotnet\control-plane\SyLabAI.ControlPlane.sln
dotnet test .\backend\dotnet\control-plane\SyLabAI.ControlPlane.sln --no-restore -v:minimal
npm --prefix .\apps\web run build
git diff --check
```

If a check cannot be run because the scaffold does not exist yet, say that directly in the change summary.
