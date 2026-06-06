# SyLabAI Control Plane

This directory is reserved for the ASP.NET Core backend solution.

Planned projects:

- `src/SyLabAI.ControlApi`
- `src/SyLabAI.Application`
- `src/SyLabAI.Domain`
- `src/SyLabAI.Infrastructure.AI`
- `src/SyLabAI.Infrastructure.Documents`
- `src/SyLabAI.Infrastructure.Sqlite`
- `src/SyLabAI.Worker`
- `tests/SyLabAI.ControlApi.Tests`

The Control API should expose stable DTO-based endpoints. Provider calls, document conversion, retrieval, persistence, and file system access should remain behind explicit application and infrastructure boundaries.
