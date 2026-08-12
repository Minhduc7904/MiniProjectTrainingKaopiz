# API Documentation Rules

- Store an API contract in `docs/api/<owning-service>/`; never document an endpoint under a caller service.
- Create or update the endpoint documentation in the same change as endpoint implementation, request/response changes, status-code changes, or authorization changes.
- Use `docs/api/_templates/endpoint.md` for every new endpoint document or section.
- Every endpoint must state its purpose, owner service, method, path, authentication, authorization/ownership rule, request schema, a concrete request example, response schema, a concrete success response, validation, business preconditions, status/error codes, and side effects.
- All JSON success and error examples must use `docs/api/shared/response-format.md`; do not invent endpoint-specific response wrappers.
- Document cursor pagination with `meta.pagination.type`, `limit`, `nextCursor`, and `hasNextPage`; document offset pagination with `page`, `pageSize`, `totalItems`, and `totalPages`.
- Document pagination ordering and cursor behavior for list APIs.
- Document idempotency, retry, asynchronous processing, or concurrency behavior when an endpoint uses any of them.
- Reuse the safe error contract in `docs/api/shared/error-format.md`; never expose stack traces, SQL, credentials, or object-storage keys.
- Keep API docs synchronized with `docs/database/`, `docs/business-flows/`, and `docs/architecture/` when the contract affects those areas.
