# Database Migration

## Use when

Changing persisted schema, indexes, constraints, or stored data.

## Procedure

1. Read `rules/code-quality.md`, `rules/testing.md`, and the relevant database documentation.
2. Document the purpose, forward change, compatibility impact, and rollback approach in `docs/database/`.
3. Create a reversible migration when the chosen framework supports it.
4. Validate the migration against representative data and add tests for affected behavior.
5. Record operational prerequisites, data backfill steps, and rollback conditions in `docs/runbooks/` when needed.
