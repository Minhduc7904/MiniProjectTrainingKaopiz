---
name: init.api.graphql
stacks: ["node","python","php","go","rust"]
description: Use when designing a GraphQL API to handle complex, nested queries and provide client-specified data shapes.
when_to_use: Activate when the application has a complex UI with deeply nested data requirements.
license: MIT
---

# Init — GraphQL API

**Skill id:** `init.api.graphql`

GraphQL is about the graph of your data. Let the client decide what it needs.

## Iron Law — No "Fat" Resolvers

Resolvers should be thin wrappers around your service layer. Business logic belongs in services, not in the GraphQL entrypoint.

## Best Practices

- **Schema-First**: Define the schema (`.graphql`) before writing any code.
- **Data Loaders**: Use batching (DataLoaders) to prevent N+1 query problems.
- **Naming**: Use clear, descriptive names for types and fields. Avoid "Internal" prefixes.

## Excuse vs. Reality

| Excuse | Reality |
| :--- | :--- |
| "N+1 is fine for now, we don't have many users." | GraphQL makes N+1 trivial to trigger. It will crash your DB on Day 1. |
| "I'll just expose the whole DB schema." | This exposes internal details and creates a high maintenance burden when the DB changes. |

## See also

- `init.api.rest` — often used alongside GraphQL for simple status or file transfers.
- `init.database` — the source of the graph data.

---
**Summary:** GraphQL provides maximum flexibility for the client, but requires maximum discipline from the server.
