# Testing Rules

- Add or update automated tests for every changed behavior.
- Read `skills/test-unit/` before writing unit tests.
- Read `skills/test-component/` before writing `TestServer` endpoint or
  middleware tests.
- Read `skills/test-integration/` before writing tests that use MySQL, MinIO,
  RabbitMQ, SQL migrations, Docker, or Testcontainers.
- Keep tests deterministic and independent of shared external state.
- Use unit tests for local logic and integration tests for component boundaries.
- Component tests use `TestServer` and in-memory test doubles; they do not open
  real network or database connections.
- Integration tests must provision isolated dependencies and must never use the
  developer's Docker Compose database, buckets, broker, or seeded data.
- Update the matching `docs/tests/<service>/<type>.md` catalog with setup,
  action, expected result, and exact pass condition.
- Run the relevant test, lint, and formatting commands before handoff.
- Document a justified exception when a behavior cannot be covered by an automated test.
