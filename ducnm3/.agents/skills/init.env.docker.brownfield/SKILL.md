---
name: init.env.docker.brownfield
stacks: ["node","python","php","go","rust"]
description: Use to verify an existing Docker / compose / .env.example setup in a brownfield repo and only add containers, env vars, services, or healthchecks that are missing. Prefers dev-optimized image variants (alpine, slim, LocalStack for AWS) for local parity. Never touches `docker-compose.prod.yml`. Trigger for "audit our compose file", "our .env.example is missing vars", "pin the postgres version in dev compose", "add redis to compose without breaking what's there". If the repo has no containers at all, delegates to greenfield `init.env.docker`.
when_to_use: Step 4 of setup.init.brownfield. Skip entirely if init.enabled_phases excludes environment. If the repo has no containers at all, delegate to greenfield init.env.docker.
license: MIT
---

# Init Brownfield — Environment (Docker audit + gap-fill)

The repo may already run on containers. Do not rebuild the image strategy. Check what is in place, document it, and only add the parts that are genuinely missing.

## Iron Law — Do not break running containers

If `docker-compose.yml` and `Dockerfile` exist and work, treat them as source of truth. Any change must be backwards-compatible with existing dev laptops and staging environments.

## Inputs & Tool Discipline

Reads `.vibe/research/brownfield-init.md`:

- **§2 Runtime & dependencies** → container presence, services referenced in code, pinned vs unpinned versions.
- **§7 Gaps** → e.g. "no compose file", "no .env.example".

If either is missing, stop and request a re-run of discover. Do not re-scan the repo for services — §2 already lists what code references. Open only the compose files and Dockerfile variants discover named. Do not descend into `src/`.

To enumerate env vars actually read by code, use grep on patterns: `process.env.`, `os.environ[`, `getenv(`, `ENV[`, `System.getenv(`. Capture variable **names**; do not read full files.

## Scope gate — repo has no Docker at all

If §2 records `Containers: none`:

1. Do **not** scaffold Docker from scratch.
2. Delegate to `init.env.docker` (greenfield). Read its SKILL.md, ask the user whether to invoke it, and stop this step.
3. Record the handoff in the output summary.

The brownfield skill is for audit-and-patch; creating a container strategy from zero is a design decision that belongs in the greenfield flow.

## Apply vs Propose policy

- **Apply**: adding a missing service to compose when code clearly depends on it, generating a fresh `.env.example` when absent, adding a `healthcheck` to a service the app blocks on at startup, pinning an unpinned tag to the version already running.
- **Propose** (TODO): swapping base images, editing `docker-compose.prod.yml`, introducing a new orchestrator (k8s, nomad), changing port bindings, editing Dockerfile build stages.
- **One commit per change** — "chore(env): add Redis service to compose", "chore(env): generate .env.example". Never bundle.

## Source of truth when multiple compose files exist

Pick in this order:

1. `compose.yaml` if present (Docker's newer canonical name).
2. `docker-compose.yml` if `compose.yaml` is absent.
3. `docker-compose.dev.yml` or similar dev-suffixed files are overlays, never source of truth. They extend the base.
4. `docker-compose.prod.yml` is explicitly out of scope — document only, never edit.

If both `compose.yaml` and `docker-compose.yml` exist simultaneously, flag the duplicate in output and stop; ask the user which to treat as canonical. Do not merge them.

## Dev-optimized image preference

"Match production version" is right for **semantic version**, but the image **variant** should be dev-optimized for local parity:

| Service | Prefer for local dev | Prod match (by version only) |
|---|---|---|
| Postgres | `postgres:<major>-alpine` | `postgres:16.x` (same major) |
| Redis | `redis:<major>-alpine` | `redis:7.x` |
| MongoDB | `mongo:<major>` (no alpine variant officially) | `mongo:7.x` |
| Node base images in Dockerfile | `node:<version>-alpine` or `-slim` for dev-only stages | keep prod stage as chosen |
| AWS-dependent services (S3, SQS, DynamoDB) | `localstack/localstack` | actual AWS in prod |
| Elasticsearch | `elasticsearch:<major>` (no alpine, official) | same major |
| RabbitMQ | `rabbitmq:<major>-alpine` | `rabbitmq:3.x` |

Rules:
- Pin by **major** version to match production semver, not by full digest. Patch/minor drift is acceptable for local dev.
- Prefer `-alpine` or `-slim` when an official variant exists.
- For cloud services, prefer open-source emulators (LocalStack, Azurite) in dev compose. Never wire real cloud credentials into the dev stack.
- If the team already uses a specific variant (e.g. `postgres:16` without alpine), **keep it**. Iron Law wins over the preference table.

## Detection rules — "code clearly depends on this service"

Evidence in code, in order of confidence:

1. **Library import + connection attempt** — `import { Pool } from 'pg'`, `redis.createClient(...)`. High confidence.
2. **Env var pattern matching a known service** — `DATABASE_URL`, `REDIS_URL`, `S3_BUCKET`, `KAFKA_BROKERS`. Medium confidence.
3. **Connection string in config** — `postgres://`, `mongodb://`, `amqp://`. High confidence.
4. **Comment or README reference only** — low confidence; do not add to compose on comment alone.

Add a service to compose only at **medium or high** confidence. Low-confidence mentions go into the summary as "verify with team".

## `.env.example` generation scope

When §7 records missing `.env.example` and §2 lists env var usage:

1. Scan grep output for env var **names**.
2. Deduplicate. Group by prefix (e.g. `DATABASE_*`, `AWS_*`, `FEATURE_*`).
3. Write `.env.example` with every name, each value empty or a placeholder:
   ```
   # Database
   DATABASE_URL=
   DATABASE_POOL_MIN=

   # Cache
   REDIS_URL=

   # Feature flags
   FEATURE_NEW_ONBOARDING=false
   ```
4. **Never put real secrets** — not even short-lived ones. Placeholders only.
5. If a var has a safe default visible in code (e.g. `process.env.PORT || 3000`), record that default. Otherwise leave empty with a `# required` comment.

`.env.example` must **not** be git-ignored (the point is to commit it). `.env` (real values) must be ignored.

## Finding the production version

To pin unpinned services to the version running in production, check in this order:

1. `docker-compose.prod.yml` or `compose.prod.yaml` (read-only — do not edit).
2. Deploy scripts / terraform / k8s manifests referenced in §4 (CI/CD).
3. README "requirements" section.
4. Team confirmation (record as TODO if no evidence exists).

Never pin based on latest-available. Pin based on what is actually running.

## Implementation Steps

1. **Gate check** — if §2 shows no containers, delegate to greenfield and stop.
2. **Pick source of truth** — identify the one compose file to treat as primary. Flag duplicates.
3. **Audit**:
   - Services declared vs services referenced in code (§2).
   - Pinned vs unpinned versions.
   - `.env.example` presence vs env vars used in code.
   - Healthchecks on services the app blocks on.
   - Port conflicts (compose vs local convention; compose vs CI job ports).
4. **Build gap list** — one row per gap.
5. **Classify Apply vs Propose**.
6. **Apply**:
   - Add missing services using dev-optimized image preference rules.
   - Pin unpinned tags using production-version lookup.
   - Generate `.env.example` using scope rules.
   - Add healthchecks.
   - One commit each.
7. **Record TODOs** for propose-only items.
8. **Validate** — acceptance criteria including a `docker compose config` parse.

## Output

Two artifacts:

1. **Commits** — one per gap-fill.
2. **Summary** appended to `.vibe/research/brownfield-init.md` under `## Environment outcome` (or `.vibe/research/env-alignment.md`):

```markdown
## Environment outcome

### Source of truth
- Primary compose file: <path>
- Flagged duplicates: <list or none>

### Audit
| Item | State before | State after | Action |
| --- | --- | --- | --- |
| Postgres service | unpinned `postgres:latest` | pinned `postgres:16-alpine` | applied |
| `.env.example` | missing | generated with 14 vars | applied |
| `docker-compose.prod.yml` | deploy-target mismatch | left alone | proposed (TODO) |

### Deferred / proposed
- Swap base image in Dockerfile (needs team approval).
```

## Forbidden

- Swapping base images to shrink size unless the team explicitly asked.
- Touching `docker-compose.prod.yml` or any `*.prod.yml` — document only.
- Introducing a new orchestrator (k8s, nomad) in this step.
- Creating Docker from scratch when repo has none — delegate to greenfield.
- Pinning to `latest` or leaving services unpinned.
- Putting real secrets in `.env.example`.
- Editing `docker-compose.yml` and `.env.example` in the same commit.

## Acceptance criteria

Confirm all of the below:

1. §2 and §7 of discover were the source of truth.
2. If `§2.Containers = none`, this step delegated to greenfield and stopped. Otherwise all gaps are listed and resolved (applied or deferred).
3. `docker compose config -f <primary file>` parses without error. Record the command + result in the summary.
4. Every added service uses a pinned, dev-optimized tag per the preference table (or justifies deviation in the summary).
5. If `.env.example` was generated or edited, it contains only placeholders and is committed (not git-ignored).
6. Every change is in its own commit.
7. Deferred / proposed items are listed with explicit reasons.

## See also

- `init.env.docker` — greenfield baseline. Delegate here if the repo has no Docker.
- `init.brownfield.discover` — reads §2 and §7 as input.
- `init.pipeline.brownfield` — ensures CI uses the same images and tags.
