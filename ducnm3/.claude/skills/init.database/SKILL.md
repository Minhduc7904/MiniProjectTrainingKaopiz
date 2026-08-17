---
name: init.database
stacks: ["node","python","php","go","rust"]
description: Use when designing the data model, selecting a database engine, and planning migrations for a new project.
when_to_use: Activate after high-level architecture is defined, alongside API design. Mandatory before writing any persistence-related code.
license: MIT
---

# Init — Database design

**Skill id:** `init.database`

The database is the "source of truth". A flawed schema is the hardest technical debt to pay back.

## Iron Law — Schema-First, Code-Second

Never write application logic that assumes a schema that hasn't been formalized in a migration or a design doc. Data integrity is the highest priority.

## Design Checklist

- **Normalization**: Start with 3NF. Denormalize only when performance data proves it's necessary.
- **Constraints**: Use Foreign Keys, Not Null, and Check constraints at the DB level, not just the app level.
- **Migrations**: Every schema change must be a versioned script. No manual `ALTER TABLE` in production.

## Excuse vs. Reality

| Excuse | Reality |
| :--- | :--- |
| "I'll just use JSON fields and fix the schema later." | JSON fields prevent the DB from enforcing integrity and make querying slow/complex. |
| "Migrations are too slow for fast iterations." | Migrations are the only way to ensure the dev environment matches production. |

## See also

- `init.architecture` — the broader system context for the database.
- `phase.plan.risk-assess` — assessing the risk of schema changes.

---
**Summary:** Solid data models lead to predictable application logic.
