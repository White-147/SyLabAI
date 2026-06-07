# SyLabAI Web App

This directory contains the React + TypeScript frontend workspace.

Planned feature areas:

- `dashboard`
- `document-library`
- `knowledge-chat`
- `experiment-records`
- `path-suggestions`
- `lab-tasks`
- `settings`

Frontend code should call backend capabilities only through Control API wrappers. It must not directly read local files, SQLite databases, provider SDKs, API keys, or document converter internals.

## Commands

```powershell
npm install
npm run dev
npm run build
```

The default API base URL is `http://127.0.0.1:5200`. Override it with `VITE_SYLABAI_API_BASE_URL` when needed.
