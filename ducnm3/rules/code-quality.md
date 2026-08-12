# Code Quality Rules

- Follow the project's formatter and linter after a stack is selected.
- Prefer small, cohesive modules with explicit interfaces.
- Do not add dependencies without a clear need and documented rationale.
- Handle expected failures explicitly; do not silently discard errors.
- Keep secrets, credentials, and machine-specific configuration out of source control.
- Use shared API constants for response error codes, headers, paths, and health-status values; keep business constants in the owning service rather than duplicating magic strings.
- Update `docs/architecture/` when a change modifies component boundaries or significant design decisions.
