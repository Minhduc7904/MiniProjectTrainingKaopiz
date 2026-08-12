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
3. If the task matches a repeatable workflow, read and follow `skills/<skill-name>/SKILL.md`.
4. Inspect the existing implementation before editing it.
5. Add or update tests and documentation when behavior, API, database, or architecture changes.

## Documentation map

- `docs/architecture/`: system boundaries and architecture decisions.
- `docs/api/`: API contracts and endpoint behavior.
- `docs/business-flows/`: actor-facing business flows and expected data changes.
- `docs/database/`: schema, data rules, and migrations.
- `docs/development/`: environment setup and development workflow.
- `docs/plan/`: five-day implementation plan and daily deliverables.
- `docs/runbooks/`: operational and recovery procedures.
- `rules/`: concise requirements that apply to implementation and reviews.
- `skills/`: step-by-step procedures for recurring work.

## Project layout

- `backend/`: backend services, workers, shared backend code, and backend tests.
- `frontend/`: web client and frontend tests.
- `tests/`: cross-service integration and end-to-end tests.
- `scripts/`: local development and CI helper scripts.
