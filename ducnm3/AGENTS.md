# Agent Guide

## Scope and Git flow

- This project is contained in `ducnm3/`. Only edit files in this directory unless the user explicitly requests otherwise.
- `ducnm3` is the integration branch. Merge every ticket pull request into `ducnm3`.
- From Day 3 onward, follow `.agents/skills/developer-task/` and
  `docs/guide/DEV_TASK_GUIDE.md`.
- Do not push **code** when the matching `docs/plan/` task has Ticket `Chưa tạo`
  or no Jira key.
- Documentation-only markdown (`docs/**/*.md`, `AGENTS.md`, `rules/*.md`,
  `.agents/**/*.md`) may be committed and pushed directly to `ducnm3`. No ticket
  branch and no pull request. If the diff includes application code, use the
  ticket-branch flow.
- Create or switch to a branch named `feature/{backlog-key}`, for example
  `feature/BLD-124_4`. Base it on `ducnm3`.
- Do not use `feature/ducnm3_<short-description>` from Day 3 onward.
- Commit message phải viết bằng tiếng Việt, ngắn gọn nhưng nêu rõ hành động và
  phạm vi thay đổi chính. Ưu tiên dùng tiêu đề task backlog nếu tiêu đề đã rõ
  nghĩa; không dùng `feat:` prefix hoặc message chung chung như `update`,
  `fix`, `wip`.
- When the user asks, create a pull request targeting `ducnm3`. PR title is the
  backlog task title. PR body must follow the Vietnamese template in
  `docs/guide/GIT_GUIDE.md` (Tổng quan, Trước chỉnh sửa, Sau chỉnh sửa, Nội dung
  chỉnh sửa, DB). Keep technical terms in English.
- Review a pull request only when the user asks. The user may merge without a
  review.

## Required reading before a change

1. Read the relevant file in `rules/`.
2. Read the applicable design or behavior documentation in `docs/`.
3. Select the mandatory workflow skill from the matrix below.
4. Read the matching folder under `.agents/skills/`. For MiniProject workflow
   skills, read `SKILL.md`, `reference.md`, and `template.md`. For installed
   third-party skills such as `interface-design`, read every file in that
   folder, starting with `SKILL.md`.
5. If multiple workflows apply, read every matching skill before editing code.
6. Inspect the existing implementation before editing it.
7. Add or update tests and documentation when behavior, API, database, or architecture changes.

Do not start coding, migration, tests, API docs, business-flow docs, or Postman
changes until the mandatory skill files have been read.

All project skills live in **one place**: `.agents/skills/`. See
`.agents/README.md`. Do not look for skills in `skills/`.

## Mandatory skill routing

- GET resource detail: `.agents/skills/api-get-detail-endpoint/`.
- GET collection/list/search: `.agents/skills/api-get-list-endpoint/`.
- POST endpoint: `.agents/skills/api-post-endpoint/`.
- PUT endpoint: `.agents/skills/api-put-endpoint/`.
- PATCH endpoint: `.agents/skills/api-patch-endpoint/`.
- DELETE endpoint: `.agents/skills/api-delete-endpoint/`.
- Unit tests: `.agents/skills/test-unit/`.
- Component tests with `TestServer`: `.agents/skills/test-component/`.
- Integration tests with real dependencies/Testcontainers:
  `.agents/skills/test-integration/`.
- SQL schema migration or EF scaffold: `.agents/skills/database-migration/`.
- Release/deployment workflow: `.agents/skills/release/`.
- Jira/WBS task, estimate, ticket branch, push gate, or PR review from Day 3:
  `.agents/skills/developer-task/`.
- Product UI, dashboard, admin panel, frontend visual work:
  `.agents/skills/interface-design/` và `rules/frontend.md`.
- Trang frontend một API (menu sidebar, Workbench Input/Output, clone
  Sổ học viên): `.agents/skills/frontend-api-page/` cộng
  `interface-design` và `rules/frontend.md`.

An endpoint implementation normally requires one HTTP-method skill plus the
applicable test skills. A schema-changing endpoint also requires the database
migration skill. Một page frontend gọi API còn cần `frontend-api-page`. From
Day 3 onward, also read `developer-task` for the matching plan item.

## Documentation map

- `docs/architecture/`: system boundaries and architecture decisions,
  including `docs/architecture/frontend.md`.
- `docs/api/`: API contracts and endpoint behavior.
- `docs/business-flows/`: actor-facing business flows and expected data changes.
- `docs/database/`: schema, data rules, and migrations.
- `docs/development/`: environment setup and development workflow.
- `docs/guide/`: practical setup and operational guides.
- `docs/plan/`: five-day implementation plan and daily deliverables.
- `docs/runbooks/`: operational and recovery procedures.
- `rules/`: concise requirements that apply to implementation and reviews.
- `.agents/skills/`: step-by-step procedures for recurring work. Read this
  folder before coding.

## Project layout

- `.agents/skills/`: every project skill. Read `AGENTS.md` then the matching
  skill before coding.
- `backend/`: backend services, workers, shared backend code, and backend tests.
- `frontend/`: web client and frontend tests.
- `tests/`: cross-service integration and end-to-end tests.
- `scripts/`: local development and CI helper scripts.

<!-- devkit:begin — managed by devkit-sync, do not edit -->
## DevKit rules

Read the matching file when the task touches it — do not load them all up front.

| Rule | File |
| --- | --- |
| Default MCP macro for “implement/execute task” when no active Vibe session; defers to resolved per-session current_instruction when a session applies | `.agents/rules/vibe-coding-workflow.md` |
| kaopiz-devkit session — resolve task (--task → chat/context → DEVKIT_TASK_ID/docs → active.json); instruction contract is per-session files under .vibe/sessions/ | `.agents/rules/kaopiz-devkit-current-instruction.md` |
| DevKit slash command index — research / plan / verify (agent-centric shortcuts) | `.agents/rules/devkit-commands.md` |
| kaopiz-devkit-skill-scan-after-write | `.agents/rules/kaopiz-devkit-skill-scan-after-write.md` |
| Minimum structure for .vibe/research and per-task PLAN — spec/plan analysis log (on-disk artifacts; does not replace DevKit JSONL) | `.agents/rules/artifact-spec-plan-log.md` |
| Principles for designing Cursor slash commands (agent orchestration) and the CLI↔Agent contract — use when developing Hub commands, rules, or kaopiz-devkit handoff | `.agents/rules/agent-orchestrated-commands.md` |
| SBU2 runtime audit-logging contract — applies ONLY when runtime_engine=sbu2-ai-kit and you dispatch subagents via MCP batch_invoke_subagents. When runtime_engine=devkit, this rule does not apply and … | `.agents/rules/sbu2-audit-logging.md` |

<!-- devkit:end -->
