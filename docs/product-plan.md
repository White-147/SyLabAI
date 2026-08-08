# Product Plan

SyLabAI is intended as an internal lab AI assistant for experiment knowledge work.

## MVP Goal

Build a Windows Server-friendly internal tool that can:

- ingest lab documents and historical records,
- answer questions with citations,
- structure experiment conditions and results,
- draft experiment path suggestions,
- create lab task cards or SOP drafts,
- collect result feedback for future iterations.

## Primary Users

- Lab staff who need to search SOPs, records, reports, product data, or reference material.
- Internal technical staff who prepare experiment paths and summarize evidence.
- Operators who need task cards or execution notes, but still rely on human lab expertise.

## Core Screens

- Dashboard.
- Document library.
- Knowledge chat.
- Experiment records.
- Path suggestions.
- Lab tasks.
- Settings.

## First Milestone

The first useful milestone should prove:

1. A document can be uploaded and parsed.
2. Parsed text can be chunked and searched.
3. A user question can receive a source-grounded answer.
4. An experiment record can be converted into structured fields.
5. A path suggestion can be produced with evidence, assumptions, and risks.

Current implementation note:

- Controlled text ingestion is persisted in SQL Server and indexed for source retrieval.
- File parser integration is represented by a converter dry-run boundary; full uploaded-file parsing still requires the later parser adapter.
- DeepSeek provider configuration supports protected API key storage, provider model listing, and connectivity tests; generation calls remain explicitly disabled by default.
- SQLite is not retained as a runtime database because the product needs a path toward 100GB+ tables and larger analytical workloads.

## Safety Positioning

Every path suggestion is a draft. The product must keep human review visible and avoid presenting AI output as final experimental truth.
