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
data                                      # project-local runtime data, ignored
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

The backend uses SQL Server as the only runtime database. SQL Server Express or Developer Edition is sufficient for local development. Configure it with one of:

```text
ConnectionStrings:SyLabAI
SyLabAI:SqlServer:ConnectionString
SYLABAI_SQLSERVER_CONNECTION_STRING
```

Document metadata, chunks, lab tasks, and search indexes live in SQL Server. The UI must continue to access this data only through Control API DTOs. The infrastructure layer creates the configured database when the login has permission, then initializes the application tables and indexes.

Example local process configuration:

```powershell
$env:SYLABAI_SQLSERVER_CONNECTION_STRING = "Server=localhost\\SQLEXPRESS;Database=SyLabAI;Trusted_Connection=True;TrustServerCertificate=True;"
dotnet run --project .\backend\dotnet\control-plane\src\SyLabAI.ControlApi\SyLabAI.ControlApi.csproj
```

Do not commit real database passwords, dumps, backups, or exported lab data.

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

The web app uses React Router. `apps/web/src/app` owns the route table and application shell, while each feature route lives under `apps/web/src/features/<feature>/<FeaturePage>.tsx`. The left rail should navigate between routes such as `/dashboard`, `/documents`, `/knowledge`, `/experiments`, `/suggestions`, `/tasks`, and `/settings`; avoid adding new feature work back into one large workspace component.

## Document Converter

`backend/services/document-converter` is still reserved for a narrow document parsing boundary. The current backend uses a simple in-process chunker for controlled text ingestion and exposes a dry-run converter check:

```text
POST /api/documents/conversions/dry-run
```

The dry-run endpoint accepts only file metadata (`fileName`, `contentType`, `sizeBytes`) and returns acceptance, rejection reasons, and safety checks. It does not read local files, write uploads, call Python, or expose local paths.

MarkItDown or another parser may be added later, but it must not become the default application control plane.

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

DeepSeek provider settings support status, local key storage, provider model listing, and connectivity tests through Control API DTOs. The key value is never returned to the frontend.

The backend reads keys from the following sources:

```text
SyLabAI:Provider:ApiKey
DEEPSEEK_API_KEY
SYLABAI_PROVIDER_API_KEY
```

Default provider configuration:

```text
BaseUrl: https://api.deepseek.com
Model: deepseek-v4-pro
EnableLiveCalls: false
```

Set a key only in the current process or ignored local configuration when testing:

```powershell
$env:DEEPSEEK_API_KEY = "<redacted>"
dotnet run --project .\backend\dotnet\control-plane\src\SyLabAI.ControlApi\SyLabAI.ControlApi.csproj
```

Live calls remain disabled unless `SyLabAI:Provider:EnableLiveCalls` is explicitly set to `true`.

The web settings page saves Base URL/API Key through the backend, then requests `GET /api/settings/provider/models` to populate the model selector from the provider. A frontend `Control API 未连接` message means the backend service is unavailable; provider status codes such as authentication failure, balance/quota failure, rate limiting, or timeout come from the backend Provider boundary.

## Verification

Use the smallest sufficient check:

```powershell
dotnet build .\backend\dotnet\control-plane\SyLabAI.ControlPlane.sln
dotnet test .\backend\dotnet\control-plane\SyLabAI.ControlPlane.sln --no-restore -v:minimal
npm --prefix .\apps\web run build
git diff --check
```

Or run the bundled Windows verifier:

```powershell
.\scripts\windows\verify.ps1
```

Provider, document conversion, and retrieval behavior should be verified first with synthetic fixtures.
