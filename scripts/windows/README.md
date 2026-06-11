# Windows Scripts

This directory is reserved for Windows setup, run, verification, and future service-management scripts.

Scripts should:

- infer paths from the repository root where possible;
- keep runtime data in project-local folders;
- avoid machine-wide environment or PATH changes unless explicitly requested;
- distinguish Machine/System PATH, User PATH, and current-process PATH before changing environment variables;
- avoid printing secrets, local private paths, or raw provider payloads.

## Verification

Run the local verification bundle from the repository root:

```powershell
.\scripts\windows\verify.ps1
```

The script runs:

- `dotnet build` for the Control API solution;
- `npm --prefix .\apps\web run build` for the web app;
- `git diff --check`.

It uses `.cache\nuget` for the current process and clears only the current-process `npm_config_store_dir` variable before invoking npm. It does not modify User/Machine environment variables.
