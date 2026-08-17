---
name: init.env.docker
stacks: ["node","python","php","go","rust"]
description: Use when containerizing an application and its dependencies (DB, Cache) for consistent development and deployment environments.
when_to_use: Activate during project setup. Mandatory for all B2B or production-bound applications.
license: MIT
---

# Init — Environment (Docker)

**Skill id:** `init.env.docker`

"It works on my machine" is a failure state. Docker is the solution.

## Iron Law — Production-Parity

The `docker-compose.yml` used for local development must mirror the production environment as closely as possible (same versions, same base images).

## Best Practices

- **Multi-stage Builds**: Separate build-time dependencies from the final lightweight runtime image.
- **Layer Optimization**: Order commands to maximize Docker's layer caching (e.g., `copy package.json` before `copy src/`).
- **Isolation**: Never use `root` in the final image. Use a non-privileged `node` or `app` user.

## Excuse vs. Reality

| Excuse | Reality |
| :--- | :--- |
| "Docker is too heavy for local dev." | Debugging environment-specific bugs in production is 100x heavier. |
| "I'll containerize it later before we deploy." | Changing the runtime environment late in the cycle introduces massive risk. |

## See also

- `init.pipeline` — using these Docker images in the CI/CD pipeline.
- `setup.project-init` — setting up `.dockerignore` during project init.

---
**Summary:** Containers are the atomic unit of modern deployment.
