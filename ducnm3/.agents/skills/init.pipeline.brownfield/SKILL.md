---
name: init.pipeline.brownfield
description: Use to audit an existing CI/CD pipeline in a brownfield repo (GitHub Actions, GitLab CI, Jenkinsfile, Buildkite, CircleCI) and only add missing gates (test / lint / image-pin / security scan). Never rewrites a pipeline that already runs. Refuses to edit CI DSL syntax the agent is not 100% sure of — inserts a `# TODO` comment instead. Trigger for "add a test job to our CI", "our GitHub Actions workflow is missing lint", "audit our pipeline gates", "we have no security scan yet". If the repo has no CI at all, delegates to greenfield `init.pipeline`.
when_to_use: Step 5 of setup.init.brownfield. Skip entirely if init.enabled_phases excludes pipeline. If the repo has no CI at all, delegate to greenfield init.pipeline.
license: MIT
---

# Init Brownfield — Pipeline (audit + gap-fill)

A pipeline that ships code every day is more valuable than a perfect one on paper. Audit first; gap-fill only the missing stages; never break what already runs.

## Iron Law — Never silently disable a working gate

If CI already enforces a check (lint, test, security scan), do not bypass or replace it. Every change must keep or strengthen existing enforcement.

## Inputs & Tool Discipline

Reads `.vibe/research/brownfield-init.md`:

- **§4 CI/CD** → one row per pipeline file with triggers and stages observed.
- **§7 Gaps** → e.g. "no CI", "tests missing from CI", "no security scan".

If either is missing, stop and request a re-run of discover. Do not re-scan the repo for pipeline files. When editing a pipeline file, open **only** that file. Do not read the scripts it invokes unless needed to verify cross-reference with step 3 (see below). Use dry-parse tools when available for the platform (see Acceptance Criteria).

## Scope gate — repo has no CI at all

If §4 is empty and §7 records "no CI":

1. Do **not** scaffold CI from scratch.
2. Delegate to `init.pipeline` (greenfield). Read its SKILL.md, ask the user whether to invoke it, and stop this step.
3. Record the handoff in the output summary.

## CI syntax safety — mandatory rule

LLMs hallucinate CI DSLs. Jenkinsfile declarative vs scripted, GitHub Actions step syntax, GitLab CI includes/extends, Buildkite pipeline YAML — each has gotchas that look right but break in subtle ways.

**Before editing any pipeline file, self-assess certainty.**

| Certainty | Action |
|---|---|
| 100% sure of the platform's syntax for this edit | Edit the file directly. |
| Unsure about any part | Do **not** edit. Insert a platform-appropriate `# TODO: ...` comment at the location where a human should add the job, describing the desired behavior in plain English. Record the gap in the summary as "needs human or platform-specialist to apply". |

GitHub Actions is the most common and best-supported — edits there are usually safe. **Jenkinsfile (especially scripted Groovy), complex GitLab `extends` trees, and Buildkite with plugins are common fail points — default to TODO comments unless the edit is trivial** (e.g. adding a single script line to an existing test job).

Never add a CI job that references commands you have not confirmed exist (e.g. an `npm run lint` job when no `lint` script is in `package.json`).

## Apply vs Propose policy

- **Apply**: adding a test job when a test script exists, adding a lint job when lint is configured and a script exists, pinning an `actions/*` version to match production runner, adding a changed-files filter to an existing job.
- **Propose** (TODO comment only): edits to Jenkinsfile / Buildkite / complex GitLab flows you are not certain of, new deploy steps, secret wiring, branch protection changes.
- **One commit per job added.** Bundling "add lint + test + security scan" into one commit is forbidden — each must be independently revertable.

## Gap-fill priority + cross-reference with step 3

Gap-fill order, because each unlocks the next:

1. **Test job** — nothing else matters without this. Run the existing test command on PR trigger.
2. **Lint job** — cheap to run, protects review quality. Use changed-files-only scope introduced by step 3 (warn-only landing → error after ratchet).
3. **Image pin parity** — if `init.env.docker.brownfield` pinned compose services, CI jobs using container services must reference the same tags. Example: `services: postgres:16-alpine` in CI matches compose.
4. **Security scan** — secrets scan (gitleaks, trufflehog) or dep vuln scan (`npm audit --audit-level=high`, `pip-audit`). **Default OFF** until the team confirms capacity to triage findings. Record a TODO to enable once someone owns triage.

Every CI job must invoke **the same command** as the local developer script. If step 3 added `npm run lint:ci` for changed-files-only linting, the CI lint job must call `npm run lint:ci` — not re-implement the lint command inline.

Before adding a job:

1. Read `package.json` scripts (or equivalent). Confirm the exact script name.
2. Reference that script in the job. Do not duplicate its logic into CI YAML.
3. If the script does not exist yet, record a TODO: "Job stub added; wait for `init.convention.align.brownfield` to land the matching `lint:ci` script."

This keeps local/CI parity and means CI changes stay one-line when a script name changes.

## "Working gate" — what counts

An existing job is a "working gate" **only if**:

- It runs on every PR (not just `main` pushes, not only scheduled).
- It blocks merge on failure (no `continue-on-error: true`, no `allow_failure: true` on GitLab, no `|| true` swallowing exit codes).
- Its command actually exits non-zero on detectable problems (not just logs a warning).

A job with `continue-on-error: true` is **not** a working gate — it is a weak gate. You may add a stronger replacement alongside it (with a different job name) and leave the weak one in place with a TODO. Do not silently remove the weak gate; the team may have reasons for the soft-land.

## Branch protection — out of scope for file edits

CI-config files cannot enable branch protection on GitHub / GitLab — those are UI/API settings. Therefore:

- **Document** the required branch protection in `CONTRIBUTING.md` or equivalent (handoff to `init.documentation.brownfield` via output summary).
- Do **not** claim branch protection is enforced because a workflow file exists.
- Record in the output: "Branch protection for `main` must be enabled by repo admin — required reviews: N, required checks: <list of jobs this step added>."

## Secrets handling

Secrets cannot be added from a repo edit — they live in the CI provider's secret store. Therefore:

- Document every required secret in the output summary: name · purpose · which job consumes it.
- If a job is added that expects a secret that does not exist, mark the job with a conditional (e.g. GHA: `if: ${{ secrets.FOO }}`) so it skips when the secret is missing, with a TODO comment.

## Multiple CI providers

If §4 lists more than one pipeline provider (e.g. GitHub Actions + CircleCI):

1. Identify the primary — whichever is referenced in branch protection, README, or has the most recent commits.
2. Apply gap-fill to the primary only.
3. Record the secondary: keep it running (Iron Law), flag for migration discussion as a TODO.
4. Do not synchronize jobs between two providers — that is a migration project.

## Implementation Steps

1. **Gate check** — if §4 is empty, delegate to greenfield and stop.
2. **Build audit table** — one row per pipeline file: triggers · stages present · weak-gate markers.
3. **Identify primary** (multi-provider case) — everything else is hands-off.
4. **Score self-certainty per file** — GHA usually safe to edit; Jenkinsfile etc. often TODO-only.
5. **Classify gaps by priority** (test / lint / image-pin / security).
6. **Cross-reference step 3 scripts** — confirm command exists in manifest.
7. **Apply edits** — one commit per job; TODO comments where uncertain.
8. **Record TODOs** — secrets, branch protection, security scan capacity, multi-provider migration.
9. **Validate** — acceptance criteria including a dry-parse of the edited file.

## Output

Two artifacts:

1. **Commits** — one per job added or edited.
2. **Summary** appended to `.vibe/research/brownfield-init.md` under `## Pipeline outcome` (or `.vibe/research/pipeline-alignment.md`):

```markdown
## Pipeline outcome

### Primary provider
<e.g. GitHub Actions — .github/workflows/ci.yml>

### Audit
| File | Triggers | Stages (before) | Stages (after) | Weak gates |
| --- | --- | --- | --- | --- |
| .github/workflows/ci.yml | push, PR | lint | lint, test | none |

### Applied
- Added `test` job invoking `npm test` (commit abc1234)
- Pinned `actions/checkout` to v4

### Branch protection required (human action)
- Enable on `main`: required reviews ≥ 1, required checks: [lint, test]

### Secrets required (human action)
- `DATABASE_URL` (used by test job if integration tests run against real DB)

### Deferred / TODO
- Jenkinsfile — not edited (uncertainty about syntax); TODO comments inserted at lines 34, 58.
- Security scan — default OFF until triage owner assigned.
- CircleCI config — secondary provider, not modified.
```

## Forbidden

- Rewriting a multi-stage Jenkinsfile "because GitHub Actions is nicer" — migration is a separate project.
- Adding deploy steps without explicit owner confirmation — broken deploys are worse than no deploys.
- Silencing failing jobs (`continue-on-error: true`, `allow_failure: true`, `|| true`) to reach green — fix the failure, add a stronger alternative, or leave it red with a visible TODO.
- Editing CI syntax you are not certain of — use a `# TODO` comment instead.
- Claiming branch protection via a workflow file — that is a platform setting.
- Committing secret values or dummy credentials into the pipeline file.
- Duplicating convention-layer logic (lint rules, test commands) inline in CI — call the npm / poetry / make script instead.

## Acceptance criteria

Confirm all of the below:

1. §4 and §7 of discover were the source of truth.
2. If `§4 = empty`, this step delegated to greenfield and stopped. Otherwise every gap is either applied, TODO-commented in-file, or deferred-with-reason in the summary.
3. Every edited pipeline file dry-parses clean:
   - GitHub Actions: `gh workflow view` or `act -n` (if installed).
   - GitLab CI: `gitlab-ci-lint` / `glab ci lint`.
   - Jenkinsfile: `jenkins-cli declarative-linter` (only if certainty was 100%; otherwise the file was not edited).
   - Buildkite: `buildkite-agent pipeline upload --dry-run`.
   Record the command + result in the summary.
4. Every new job references a pre-existing script (cross-ref step 3). No inline command duplication.
5. No existing working gate was removed or weakened.
6. One commit per job.
7. Branch protection + secrets are documented as human-required, not claimed done.
8. TODO comments use the platform's comment syntax (`#`, `//`, `/* */`) — not generic.

## See also

- `init.pipeline` — greenfield baseline. Delegate here if the repo has no CI.
- `init.brownfield.discover` — reads §4 and §7 as input.
- `init.convention.align.brownfield` — owns the local scripts this step's CI jobs invoke. Cross-reference before adding a job.
- `init.env.docker.brownfield` — CI services must use the same pinned tags as compose.
- `init.documentation.brownfield` — documents required branch protection + secrets in CONTRIBUTING.md.
