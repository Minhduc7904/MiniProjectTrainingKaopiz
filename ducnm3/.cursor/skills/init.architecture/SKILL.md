---
name: init.architecture
description: Use when defining high-level system boundaries, major components, and architectural trade-offs for a new project.
when_to_use: Activate during the discovery or initial design phase, before any code is written. Mandatory for defining the long-term "shape" of the application.
license: MIT
---

# Init — Architecture

**Skill id:** `init.architecture`

Architecture is about the decisions that are hard to change later. Focus on boundaries and trade-offs.

## Iron Law — No "Implicit" Architecture

Every major architectural decision must be documented in an ADR (Architecture Decision Record). If it isn't written down, it isn't a decision; it's an accident.

## Implementation Steps

1.  **Classification**: Identify the product type (MVP, SaaS, Internal Tool) and its scale needs.
2.  **Context Mapping**: Draw the boundaries between the system and external entities (Auth, Payments, DB).
3.  **Trade-off Analysis**: Compare at least two approaches (e.g., Monolith vs. Microservices) and justify the choice.
4.  **ADR Creation**: Write a 1-page ADR for the data store, API style, and core stack.

## Excuse vs. Reality

| Excuse | Reality |
| :--- | :--- |
| "It's just an MVP, we don't need architecture." | Bad MVP architecture makes the pivot or the scale-up phase a total rewrite. |
| "Everyone knows we use React/Node." | Documenting the *why* helps future team members understand constraints. |

## See also

- `init.database` — the persistence layer of the architecture.
- `phase.spec.brainstorm` — the design process that feeds into these architectural decisions.

---
**Summary:** Good architecture is about deferring decisions until you have enough data, but documenting the ones you MUST make now.
