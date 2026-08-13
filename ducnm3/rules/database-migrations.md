# Database Migration Rules

- Read `skills/database-migration/SKILL.md`, `reference.md`, and `template.md`
  before changing schema or generated EF models.
- SQL migration files are the schema source of truth.
- Never edit a migration recorded in `schema_migrations`; create the next
  `V###__description.sql`.
- Do not place development seed data in migration files.
- Apply the migration before running the owning service's scaffold workflow.
- Scaffold only tables owned by that microservice.
- Review generated `DbContext` and entity diffs; do not hand-maintain stale
  generated mappings.
- Add integration coverage and synchronize `docs/database/` and relevant
  runbooks in the same change.
