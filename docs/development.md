# Development

This document records the local development flow. The project now has an initial runnable scaffold for the ASP.NET Core Control API and React/Vite web app.

## Prerequisites

- Windows development environment.
- .NET SDK compatible with the chosen backend target.
- Node.js compatible with the future React/Vite app.
- Python only if the document converter sidecar is enabled.
- PowerShell.
- DeepSeek API key only for explicitly authorized live API testing.

Do not store real API keys, lab documents, experiment data, or internal company material in committed files.

## Repository Layout

```text
apps/web                                  # React + TypeScript frontend
backend/dotnet/control-plane             # .NET solution
backend/services/document-converter       # optional parser sidecar
docs                                      # architecture, constraints, operations notes
scripts/windows                          # setup, run, verify helpers
data                                      # local SQLite/runtime data, ignored
uploads                                   # uploaded documents, ignored
outputs                                   # generated reports/task cards, ignored
.tools/.cache/.config/.tmp                # project-local tooling and runtime state
```

## Backend Development

Run the Control API:

```powershell
cd <repo-root>
dotnet restore .\backend\dotnet\control-plane\SyLabAI.ControlPlane.sln
dotnet build .\backend\dotnet\control-plane\SyLabAI.ControlPlane.sln
dotnet run --project .\backend\dotnet\control-plane\src\SyLabAI.ControlApi\SyLabAI.ControlApi.csproj
```

Suggested default API origin:

```text
http://127.0.0.1:5200
```

## Frontend Development

Expected command shape after scaffold:

```powershell
cd <repo-root>\apps\web
npm install
npm run dev
```

Suggested default frontend origin:

```text
http://localhost:3000
```

The frontend calls the backend through `VITE_SYLABAI_API_BASE_URL`, defaulting to `http://127.0.0.1:5200`.

## Document Converter

`backend/services/document-converter` is still reserved for a narrow document parsing boundary. The current backend uses a simple in-process chunker for synthetic text only. MarkItDown or another parser may be added later, but it must not become the default application control plane.

Expected command shape if enabled:

```powershell
cd <repo-root>\backend\services\document-converter
python -m venv .venv
.\.venv\Scripts\python -m pip install -r requirements.txt
```

The converter should accept controlled inputs and return normalized text/metadata. It should not write directly to the application database.

## Configuration

Use templates for configuration and keep real secrets out of Git.

Recommended future files:

```text
.config/appsettings.local.example.json
.config/provider.deepseek.example.json
backend/dotnet/control-plane/src/SyLabAI.ControlApi/appsettings.json
backend/dotnet/control-plane/src/SyLabAI.ControlApi/appsettings.Development.json
```

Real secrets should be supplied through environment variables or ignored local config files. Do not commit API keys, user-secrets files, or provider payloads.

## Verification

Use the smallest sufficient check:

```powershell
dotnet build .\backend\dotnet\control-plane\SyLabAI.ControlPlane.sln
dotnet test .\backend\dotnet\control-plane\SyLabAI.ControlPlane.sln --no-restore -v:minimal
npm --prefix .\apps\web run build
git diff --check
```

Provider, document conversion, and retrieval behavior should be verified first with synthetic fixtures.
