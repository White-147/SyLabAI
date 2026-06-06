# Document Converter Boundary

This directory is reserved for an optional document conversion sidecar or local adapter.

MarkItDown or similar parsers may be used here, but parser code should remain behind a controlled boundary:

- validate file type and size;
- read only approved project-local paths;
- return normalized text and metadata;
- avoid writing directly to the application database;
- never expose raw local paths or private document content in public logs.
