# devkit-workflow-profile

Configure **`devkit.workflow-profile.json`** (and optionally **`.vibe/workflow-profile.local.json`**) to:

- Store **interview answers** (`interview.answers`) for stack, API, CI, …
- **Attach skills** per init step (`init.steps[<flat_id>]`) — each `skill_id` must be a primary skill or listed in that step’s `skill_candidates` (unless `allow_non_candidate_skills: true`).
- **Skip greenfield phases** with `init.enabled_phases` (default: all 5 phases).

## Agent: Cursor chat (no TTY)

- **`devkit-workflow-profile-brownfield`** — existing codebase: MCP (`interview_mode: brownfield`) + **repo investigation** → interview → **audit** (`answer_sources` / `interview_audit` or **`workflow-profile write-with-audit`**) → finalize.
- **`devkit-workflow-profile-greenfield`** — new project: MCP (`interview_mode: greenfield`) + `default_hub_profile_hint` (stub) → interview → **audit** (`answer_sources` / `interview_audit` or **`workflow-profile write-with-audit`**) → finalize.

**UI form “Answers”:** Only when the Agent calls **`AskQuestion`** for a `select`/`confirm`/`multiselect` question that **needs** the UI; map from `interview_sections` (details in the brownfield/greenfield slash files). You can complete the interview with structured chat alone if no form is needed.

**Index:** **`devkit-answer-interview`** points to the two slashes above. MCP payload includes **`mcp_answers`**, **`agent_flow_steps`**, **`repo_investigation_hints`** (brownfield) or **`default_hub_profile_hint`** (greenfield).

## Agent: run CLI (repo root)

### 1) Infer from the repo (no LLM, no user prompts)

Read `package.json`, Docker, CI, README… → write **`devkit.workflow-profile.json`**:

```bash
npm run kaopiz-devkit -- workflow-profile infer -p .
# preview: add --dry-run
# overwrite existing file: add --force
# heuristic signals: --print-signals (stderr)
```

### 1b) Infer + write audit for every question (`answer_sources`)

Brownfield and **greenfield** share this — merges infer with the existing file and fills `interview.answer_sources` / `interview.interview_audit`:

```bash
npm run kaopiz-devkit -- workflow-profile write-with-audit -p .
# preview: add --dry-run (JSON stdout, audit stderr)
# overwrite: add --force
```

### 2) Prompt an external LLM (Cursor chat, ChatGPT, …)

Print **one markdown block** with baseline heuristics + repo snippet + instructions — user/agent pastes into an LLM and edits JSON by hand:

```bash
npm run kaopiz-devkit -- workflow-profile llm-prompt -p . > /tmp/profile-prompt.md
```

Then attach the output (or `@` the file) in chat and ask the model to return **JSON only**.

### 3) Interactive terminal interview (TTY)

Step-by-step Q&A, writes file — needs an **interactive terminal**:

```bash
npm run kaopiz-devkit -- workflow-profile interview
# or
npx kaopiz-devkit workflow-profile interview -p .
```

Preview JSON without writing: add **`--dry-run`**.

### 4) Print JSON template only (script / CI, non-interactive)

```bash
npm run kaopiz-devkit -- workflow-profile template
npx kaopiz-devkit workflow-profile template > devkit.workflow-profile.json
```

If you skip `interview`, you can ask the user using the checklist (stderr of `template`) and edit JSON by hand (or move sensitive bits to `.vibe/workflow-profile.local.json`).

**Required** after adding/changing commands in the Hub: `npm run devkit:sync` so `.cursor/commands/` stays up to date.

## Quick reference

| File | Role |
|------|------|
| `devkit.workflow-profile.json` | Main profile (commit if it contains no secrets) |
| `.vibe/workflow-profile.local.json` | Partial overlay (often gitignored with `.vibe/`) |

`kaopiz-devkit step` / step guidance merges skills from `init.steps` when valid; final init-phase `approve` suggests the **next** router per `enabled_phases`.
