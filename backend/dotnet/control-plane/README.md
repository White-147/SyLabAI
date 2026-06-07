# SyLabAI Control Plane

This directory contains the ASP.NET Core backend solution.

Projects:

- `src/SyLabAI.ControlApi`
- `src/SyLabAI.Application`
- `src/SyLabAI.Domain`
- `src/SyLabAI.Infrastructure.AI`
- `src/SyLabAI.Infrastructure.Documents`
- `src/SyLabAI.Infrastructure.Sqlite`
- `src/SyLabAI.Worker`
- `tests/SyLabAI.ControlApi.Tests`

The Control API should expose stable DTO-based endpoints. Provider calls, document conversion, retrieval, persistence, and file system access should remain behind explicit application and infrastructure boundaries.

## Current Endpoints

- `GET /api/health`
- `GET /api/documents`
- `POST /api/documents/ingestions`
- `POST /api/knowledge/search`
- `POST /api/knowledge/answers`
- `POST /api/experiments/extractions`
- `POST /api/suggestions`
- `GET /api/lab-tasks`
- `POST /api/lab-tasks`
- `GET /api/settings/provider`

The current implementation uses synthetic seed records and in-process demo storage. It does not make live provider calls or write real SQLite databases yet.

## Commands

```powershell
dotnet restore .\SyLabAI.ControlPlane.sln
dotnet build .\SyLabAI.ControlPlane.sln
dotnet run --project .\src\SyLabAI.ControlApi\SyLabAI.ControlApi.csproj
```
