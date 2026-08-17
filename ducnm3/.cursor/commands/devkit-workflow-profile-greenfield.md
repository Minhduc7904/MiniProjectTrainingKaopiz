# devkit-workflow-profile-greenfield

Configure **`devkit.workflow-profile.json`** for a **new** (**greenfield**) project: MCP returns heuristics + Hub profile hint (stub) → interview the user by section → write the file. **Does not** require deep repo archaeology unless the user already has files in the tree.

**Skill:** `setup.workflow-profile-greenfield`

## Required flow (YOU — AI)

1. **MCP**: Call `collect_workflow_profile_context` with `project_root` = repo root and **`interview_mode`: `"greenfield"`**.
2. **Summarize for the user**: `signals`, `inferred_choices`, `mcp_answers`, `profile_draft`, and **`default_hub_profile_hint`** (stub — default Hub id; teams may change after `devkit-sync` per `setup/project-profiles/*.json`).
3. **Repo**: Payload has empty `repo_investigation_hints` — **not** required to scan the codebase; only quick confirmation if `package.json` / sample files exist or the user wants it.
4. **Interview by section (`AskQuestion` only when UI choice is needed)**: You **must** cover every `question.id` (chat or form). **`AskQuestion`** — **only** when the user must pick **`select` / `confirm` / `multiselect`** via the **「Answers」** card; **not** for `text`; **not** required for every choice question if the user already answered clearly in chat. For a form-style UI, map remaining questions to **`AskQuestion`** (mapping details: **`devkit-workflow-profile-brownfield`** step 4).
   - Prefer clarifying **`init_greenfield`** (`skipped_phases`) and **daily_task_workflow**; **one section (or one `AskQuestion` group)** per turn — split across multiple calls if the form is long.
   - **No `AskQuestion` tool:** you must still cover each `question.id` in the payload (structured chat), or guide the user to run the CLI below.
5. **Merge** → `interview_choices` (form + chat for `text` questions).
6. **Audit every question (sources required)** — same as brownfield; before finalize, for **each** `question.id` in `agent_questions` / flattened from `interview_sections`:
   - Print for the user **one line** (or a table): `question_id`, chosen value, **`source`**: `user` | `inferred` | `default` | `preserved_file` | `implicit_empty`, and short `note` when auto (e.g. inferred signal, or «empty repo / greenfield default»).
   - When calling **`finalize_workflow_profile_interview`**, pass **`answer_sources`** and optionally **`interview_audit`** to write into **`devkit.workflow-profile.json`** (`interview.answer_sources`, `interview.interview_audit`).
   - **Or** user/CI: `npm run kaopiz-devkit -- workflow-profile write-with-audit -p . --force` — same CLI as brownfield (infer + merge existing file); new repos often have more **`inferred`** / **`implicit_empty`**, fewer **`preserved_file`**.
7. **Finalize**: `finalize_workflow_profile_interview` with `write_file: true`, `force_overwrite: true` if needed (and **`answer_sources` / `interview_audit`** if prepared in step 6).
8. Remind to run **`devkit-sync`**.

## Default Hub profile (extend later)

- The hint lives in MCP (`default_hub_profile_hint.id`, e.g. `backend-node`). Full mapping to `project-profiles` / env vars may be added later; it does not block finishing the interview.

## Don’t / limits

- Do not scaffold an app or write business logic in this slash — profile only.

## See also

- `devkit-workflow-profile.md` — CLI (`infer`, **`write-with-audit`**, `interview`, `template`) and output files.
