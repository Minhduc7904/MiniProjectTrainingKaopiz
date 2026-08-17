# devkit-workflow-profile-brownfield

Configure **`devkit.workflow-profile.json`** for an **existing codebase** (**brownfield**): MCP returns heuristics from the server → agent **investigates the repo** → interview the user by section (brainstorm style), then write the file.

**Skill:** `setup.workflow-profile-brownfield`

## Required flow (YOU — AI)

1. **MCP**: Call `collect_workflow_profile_context` with `project_root` = repo root and **`interview_mode`: `"brownfield"`** (or omit — default is brownfield).
2. **Summarize for the user**: `signals`, `inferred_choices`, `profile_draft`, and **`mcp_answers`** (hints with `sources` + `confidence`).
3. **Repo investigation (required)**: Use **`repo_investigation_hints`** in the payload as a checklist; read and note more (README, manifests, Docker, CI, `docs/`) to **fill or correct** weak / empty heuristics (e.g. `data_store_notes`, `domains`, confirm `api_style`). Do not skip this just because MCP returned answers.
4. **Interview by section (`AskQuestion` only when UI choice is needed)**: You **must** cover every `question.id` per `interview_sections` and do not finalize from `mcp_answers` alone (after reconciling with the repo in step 3). **`AskQuestion`** — call **only** when you need the user to answer **`select` | `confirm` | `multiselect`** via the **「Answers」** card; **do not** use it for `text`; **not** required per choice question if the user already answered equivalently in chat (with clear `question.id`). The **Answers** card exists only when the Agent calls **`AskQuestion`** — it does not auto-open from MCP.
   - For **each** `interview_sections[]` (in order): summarize `title_vi` + `description_vi` in chat if needed; **if** you need a choice form, call **`AskQuestion`** for (part or all) `select` | `confirm` | `multiselect` in that section — you may ask in chat first, then form only open questions.
   - **Mapping when calling `AskQuestion` (YOU — AI):**
     - Each question `id` in AskQuestion = **`question.id`** from the payload (e.g. `stack`, `skipped_phases`) — **keep unchanged** so `interview_choices` matches `finalize_workflow_profile_interview` schema.
     - `prompt` = `prompt_vi` (may shorten, keep the meaning).
     - **`select`:** each `options[]` → AskQuestion `options`: `id` = **`value`**, `label` = **`label_vi`**.
     - **`confirm`:** two options `{ id: "true", label: "Yes" }`, `{ id: "false", label: "No" }` (or equivalent); when building `interview_choices`, map strings → booleans.
     - **`multiselect`:** `allow_multiple: true`; `options` like `select`. Result → array (e.g. `skipped_phases`, `domains`).
     - **`text`:** AskQuestion has no free-text field — after the user submits the section form, ask **`text`** questions **in chat** (or skip if not needed).
   - Sections with **no** choice questions (e.g. `workflow_step_overrides` description-only) → no AskQuestion needed; one chat line may suffice.
   - If a section has **too many** questions for one form: **split into 2+ consecutive `AskQuestion`** calls for the same section, still using the question `id`s above.
   - **No `AskQuestion` tool:** you must still cover each `question.id` in the payload (structured chat), or guide the user to run the CLI below.
5. **Merge answers** into one `interview_choices` object (same shape as `inferred_choices`), including values from the form and from chat (text).
6. **Audit every question (sources required)** — before finalize, for **each** `question.id` in `agent_questions` / flattened from `interview_sections`:
   - Print for the user **one line** (or a table): `question_id`, chosen value, **`source`**: `user` | `inferred` | `default` | `preserved_file` | `implicit_empty`, and short `note` when auto (e.g. inferred signal).
   - When calling **`finalize_workflow_profile_interview`**, also pass **`answer_sources`** (map `question_id` → `{ source, note?, value_preview? }`) and optionally **`interview_audit`** (`schema_version: 1`, `generated_at`, `interview_definition`, `question_ids[]`) to write under **`devkit.workflow-profile.json`** as `interview.answer_sources` and `interview.interview_audit`.
   - **Or** user/CI: `npm run kaopiz-devkit -- workflow-profile write-with-audit -p . --force` — CLI walks every question in config, prints audit to **stderr**, writes profile with `answer_sources` + `interview_audit` (infer + preserve existing fields).
7. **Finalize**: `finalize_workflow_profile_interview` with `write_file: true`, `force_overwrite: true` if overwriting an old profile (and **`answer_sources` / `interview_audit`** if prepared in step 6).
8. Remind the user to run **`devkit-sync`** after the profile stabilizes.

## Don’t / limits

- Do not treat `mcp_answers` as ground truth — always reconcile with a brownfield repo.
- Do not write product code or scaffold in this step; configuration only.

## See also

- `devkit-workflow-profile.md` — CLI `infer`, `write-with-audit`, `interview`, `template`.
- MCP payload: `agent_flow_steps`, `agent_flow` (numbered).
