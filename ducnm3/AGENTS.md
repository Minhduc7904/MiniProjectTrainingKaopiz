# Agent Guide

## Scope and Git flow

- This project is contained in `ducnm3/`. Only edit files in this directory unless the user explicitly requests otherwise.
- Work directly on branch `ducnm3` by default.
- Create or switch to a feature branch only when the user explicitly asks to create one.
- Name requested feature branches `feature/ducnm3_<short-description>`, using a concise lowercase hyphenated description.
- Base a feature branch on the current `ducnm3` branch unless the user specifies another base.

## Required reading before a change

1. Read the relevant file in `rules/`.
2. Read the applicable design or behavior documentation in `docs/`.
3. Select the mandatory workflow skill from the matrix below.
4. Read its `SKILL.md`, `reference.md`, and `template.md` before editing code.
5. If multiple workflows apply, read every matching skill before editing code.
6. Inspect the existing implementation before editing it.
7. Add or update tests and documentation when behavior, API, database, or architecture changes.

Do not start coding, migration, tests, API docs, business-flow docs, or Postman
changes until the mandatory skill files have been read.

## Mandatory skill routing

- GET resource detail: `skills/api-get-detail-endpoint/`.
- GET collection/list/search: `skills/api-get-list-endpoint/`.
- POST endpoint: `skills/api-post-endpoint/`.
- PUT endpoint: `skills/api-put-endpoint/`.
- PATCH endpoint: `skills/api-patch-endpoint/`.
- DELETE endpoint: `skills/api-delete-endpoint/`.
- Unit tests: `skills/test-unit/`.
- Component tests with `TestServer`: `skills/test-component/`.
- Integration tests with real dependencies/Testcontainers:
  `skills/test-integration/`.
- SQL schema migration or EF scaffold: `skills/database-migration/`.
- Release/deployment workflow: `skills/release/`.

An endpoint implementation normally requires one HTTP-method skill plus the
applicable test skills. A schema-changing endpoint also requires the database
migration skill.

## Documentation map

- `docs/architecture/`: system boundaries and architecture decisions.
- `docs/api/`: API contracts and endpoint behavior.
- `docs/business-flows/`: actor-facing business flows and expected data changes.
- `docs/database/`: schema, data rules, and migrations.
- `docs/development/`: environment setup and development workflow.
- `docs/guide/`: practical setup and operational guides.
- `docs/plan/`: five-day implementation plan and daily deliverables.
- `docs/runbooks/`: operational and recovery procedures.
- `rules/`: concise requirements that apply to implementation and reviews.
- `skills/`: step-by-step procedures for recurring work.

## Project layout

- `backend/`: backend services, workers, shared backend code, and backend tests.
- `frontend/`: web client and frontend tests.
- `tests/`: cross-service integration and end-to-end tests.
- `scripts/`: local development and CI helper scripts.
