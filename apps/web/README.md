# SyLabAI Web App

This directory contains the React + TypeScript frontend workspace.

The app uses React Router so the left rail is an application menu, not a single-page anchor list. Each product area owns its route page, while `src/app` owns the route table and shell layout.

Current structure:

- `src/app/App.tsx`
- `src/app/routes.tsx`
- `src/app/layouts/AppShell.tsx`
- `src/app/navigation.ts`
- `src/shared/api`
- `src/shared/types`
- `src/shared/ui`

Feature route pages:

- `dashboard`
- `document-library`
- `knowledge-chat`
- `experiment-records`
- `path-suggestions`
- `lab-tasks`
- `settings`

Frontend code should call backend capabilities only through Control API wrappers. It must not directly read local files, PostgreSQL databases, provider SDKs, API keys, or document converter internals.

## Commands

```powershell
npm install
npm run dev
npm run build
```

The default API base URL is `http://127.0.0.1:5200`. Override it with `VITE_SYLABAI_API_BASE_URL` when needed.
