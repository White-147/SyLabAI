# Windows Scripts

This directory is reserved for Windows setup, run, verification, and future service-management scripts.

Scripts should:

- infer paths from the repository root where possible;
- keep runtime data in project-local folders;
- avoid machine-wide environment or PATH changes unless explicitly requested;
- distinguish Machine/System PATH, User PATH, and current-process PATH before changing environment variables;
- avoid printing secrets, local private paths, or raw provider payloads.
