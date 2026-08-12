# API Documentation

API contracts are organized by owning service. Do not document a Media Service endpoint in Course Service documentation, even when Course Service calls it.

```text
api/
├── _templates/
│   └── endpoint.md
├── shared/
│   ├── error-format.md
│   └── response-format.md
├── course-service/
│   ├── README.md
│   └── endpoints/
├── student-service/
│   ├── README.md
│   └── endpoints/
├── media-service/
│   ├── README.md
│   └── endpoints/
└── notification-service/
    ├── README.md
    └── endpoints/
```

## Required endpoint contract

Every endpoint file must include:

1. Purpose and owner service.
2. HTTP method and path.
3. Authentication and authorization conditions.
4. Request parameters/body, with a concrete request example.
5. Success response and a concrete response example.
6. All expected HTTP status codes and error codes.
7. Validation rules and business preconditions.
8. Side effects: database records, media usage, background job, or external call.
9. Pagination, idempotency, concurrency, and compatibility behavior when applicable.

Use `_templates/endpoint.md` for new endpoint files. Update the service contract in the same change as implementation.
