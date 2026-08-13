# ducnm3

## Project structure

- `AGENTS.md`: mandatory starting point for agents working in this project.
- `docs/`: architecture, API, database, development, operations, and detailed test documentation.
- `rules/`: implementation and review requirements.
- `skills/`: repeatable workflows for common engineering tasks.
- `backend/`: four business services, Scheduler API/Worker foundation, Gateway, shared building blocks, backend tests, and opt-in development tools.
- `frontend/`: web client and its UI tests.
- `tests/`: automated tests.
- `scripts/`: development and CI helper scripts, including database bootstrap/migrate/scaffold, MinIO provisioning, and guarded development data seeding.

## Getting started

The backend uses ASP.NET Core, MySQL, MinIO, YARP, NSwag, and Docker Compose.
See `docs/development/setup.md` for the local setup workflow.

To create the deterministic 100k Student/Course development dataset, see
`docs/guide/DATA_SEED_GUIDE.md`.