---
name: init.api.rest
stacks: ["node","python","php","go","rust"]
description: Use when designing a RESTful HTTP API, focusing on resources, methods, and status codes.
when_to_use: Activate during initial design, alongside database design. Mandatory for resource-oriented service interfaces.
license: MIT
---

# Init — REST API

**Skill id:** `init.api.rest`

REST is about resources. Use the HTTP protocol as it was intended.

## Iron Law — No Verbs in URLs

URLs identify resources (nouns), not actions (verbs). Use `/users`, not `/getUsers`. Actions are defined by HTTP methods (GET, POST, PUT, DELETE).

## Conventions

- **Status Codes**: 201 for Created, 400 for Bad Request, 404 for Not Found. Never return 200 for an error.
- **Filtering/Pagination**: Use query parameters (`?limit=10&offset=20`). Use cursor-based pagination for large datasets.
- **Versioning**: Put the version in the URL (`/api/v1/...`) to avoid breaking existing clients.

## Excuse vs. Reality

| Excuse | Reality |
| :--- | :--- |
| "It's easier to just use POST for everything." | This breaks caching, proxies, and developer expectations. Follow the protocol. |
| "I'll document the API later with Swagger." | Design-first API development (e.g., OpenAPI spec) prevents integration headaches. |

## See also

- `init.api.graphql` — when resource-nesting becomes too complex for REST.
- `init.database` — mapping API resources to the underlying data model.

---
**Summary:** A clean REST API is intuitive and self-describing.
