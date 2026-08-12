# Code Quality Rules

- Follow the project's formatter and linter after a stack is selected.
- Prefer small, cohesive modules with explicit interfaces.
- Do not add dependencies without a clear need and documented rationale.
- Handle expected failures explicitly; do not silently discard errors.
- Keep secrets, credentials, and machine-specific configuration out of source control.
- Update `docs/architecture/` when a change modifies component boundaries or significant design decisions.
