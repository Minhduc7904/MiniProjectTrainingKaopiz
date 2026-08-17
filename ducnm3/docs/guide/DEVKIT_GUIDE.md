# Hướng dẫn sử dụng Kaopiz DevKit trong dự án

## Mục đích và phạm vi

DevKit tổ chức workflow của Agent theo task/session, cung cấp skill/rule từ Hub
và MCP tools để lấy context, chọn workflow, kiểm tra plan/compliance. Nó không
thay thế Git, test runner, migration hay quy tắc trong `AGENTS.md`.

Project này đã có các package và scripts sau:

```bash
npm run kaopiz-devkit -- --help
npm run devkit-task-runner -- --help
npm run devkit:mcp-start
```

Lệnh trong tài liệu được chạy tại thư mục `intern_be/ducnm3`. Thêm `--help` sau
bất kỳ command nào để xem option của đúng version đang cài.

## Thành phần và file trạng thái

| Thành phần | Vị trí | Tác dụng |
| --- | --- | --- |
| Cấu hình DevKit | `devkit.config.json` | Project ID, profile, runtime engine, router, target IDE. |
| Workflow profile | `devkit.workflow-profile.json` | Stack, phase init và quy tắc daily workflow của project. |
| Task session | `.vibe/sessions/<task-id>/` | State, `PLAN.md`, `current_instruction.md`, summary và log của riêng một task. |
| Active session | `.vibe/active.json` | Trỏ đến session mặc định khi command không có `--task`. |
| Current instruction | `.vibe/sessions/<task>/current_instruction.md` | Nguồn chỉ dẫn chính xác của step đang chạy. |
| Skill đồng bộ | `.vibe/skills/` và symlink IDE | Skill lấy từ Hub; không sửa trực tiếp nếu không chủ động custom skill. |
| MCP server | `@kaopiz/vibe-coding-mcp` | Bridge để IDE Agent gọi tools DevKit. |

Một repo có thể có nhiều session song song. Với command liên quan session, luôn
truyền `--task <task-id>` khi biết task để không vô tình thao tác lên session
đang active của người khác.

```bash
npm run kaopiz-devkit -- session list
npm run kaopiz-devkit -- status --task TASK-123
npm run kaopiz-devkit -- run --task TASK-123
```

## Cài đặt và kiểm tra an toàn

### Dự án đã có mã nguồn (brownfield)

`init` tạo `.vibe/` và profile. Không dùng `--force` trong repo đang làm việc
nếu chưa kiểm tra profile hiện có.

```bash
# Chỉ dùng khi chưa có DevKit state/profile phù hợp.
npm run kaopiz-devkit -- init --mode brownfield

# Suy luận lại profile từ repository; lệnh này ghi profile.
npm run kaopiz-devkit -- workflow-profile infer -p .

# Chỉ đọc/kiểm tra workflow và state hiện tại.
npm run kaopiz-devkit -- validate
npm run kaopiz-devkit -- doctor
```

| Lệnh | Tác dụng | Có ghi file? |
| --- | --- | --- |
| `init` | Tạo `.vibe/`, `devkit.config.json`, workflow profile; tự nhận diện greenfield/brownfield. | Có. |
| `workflow-profile interview` | Hỏi đáp trong terminal rồi ghi profile. Cần TTY. | Có. |
| `workflow-profile infer` | Suy luận profile từ file repo, không dùng LLM. | Có. |
| `workflow-profile llm-prompt` | In prompt và repo snippets để dùng với Agent/LLM. | Không. |
| `workflow-profile template` | In JSON profile mẫu. | Không. |
| `workflow-profile write-with-audit` | Suy luận/merge profile và lưu audit nguồn câu trả lời. | Có. |
| `validate` | Kiểm tra workflow graph/router route. | Không. |
| `doctor` | Kiểm tra Hub kit resolution và state file. | Không. |

## Quy trình một task thông thường

### 1. Chọn router và tạo session

Router phải phù hợp công việc: `bug_fix`, `feature_dev`, `migration`,
`write_docs`, `write_test`, `code_review`... Có thể hỏi MCP tool
`suggest_common_task_routers` trước khi chọn.

```bash
npm run kaopiz-devkit -- start TASK-123 --skill write_docs \
  --task-json task.json

# Task song song: tạo session nhưng không đổi .vibe/active.json.
npm run kaopiz-devkit -- start TASK-124 --skill bug_fix \
  --task-json task.json --no-update-active
```

`task.json` có shape tối thiểu:

```json
{
  "title": "Viết hướng dẫn DevKit",
  "description": "Tài liệu cách dùng CLI, task runner và MCP tools."
}
```

`start` ghi session state, render `current_instruction.md` và thường đổi active
session. Nó không triển khai code thay Agent.

### 2. Đọc chỉ dẫn của step

```bash
npm run kaopiz-devkit -- status --task TASK-123
npm run kaopiz-devkit -- run --task TASK-123
npm run kaopiz-devkit -- step --task TASK-123
```

| Lệnh | Tác dụng | Có ghi file? |
| --- | --- | --- |
| `status` | In task, step hiện tại, trạng thái và hành động kế tiếp. | Không. |
| `run` | In đường dẫn/nội dung chỉ dẫn hiện tại để Agent đọc. | Không. |
| `step` hoặc `next` | Render lại `current_instruction.md` cho step hiện tại. | Có, chỉ file instruction. |
| `session list` | Liệt kê các session đã lưu. | Không. |
| `session activate <task-id>` | Đổi `.vibe/active.json` sang session có sẵn. | Có. |

Khi session còn `running` hoặc `awaiting_approval`, `current_instruction.md`
được ưu tiên hơn workflow tổng quát. Không tự chạy `approve` chỉ vì Agent đã
hoàn thành code; người sở hữu task phải duyệt step.

### 3. Lập kế hoạch, thực hiện và kiểm tra

Agent dùng slash command trong Cursor/Claude hoặc MCP tools tương ứng để
research/plan/verify. Trong Codex, dùng MCP tools ở phiên mới vì slash command
`.cursor/` không tự được Codex nạp.

```bash
# `verify` của version hiện tại dùng active session; activate đúng task trước.
npm run kaopiz-devkit -- session activate TASK-123
npm run kaopiz-devkit -- verify

# Chỉ kiểm tra file expected artifact, không chạy verify_commands.
npm run kaopiz-devkit -- verify --skip-commands

# Ghi nhận review thủ công trước khi approve/reject.
npm run kaopiz-devkit -- review comment --help
```

`verify` của version hiện tại chưa nhận `--task`; nó dùng active session. Vì vậy
phải kiểm tra `status --task <id>` và chỉ `session activate` khi được phép đổi
active task. `verify` chỉ có ý nghĩa khi step đã khai báo `expected_artifacts`
hoặc `verify_commands`; command đó có thể chạy lệnh kiểm tra của workflow, vì
vậy đọc `current_instruction.md` trước khi chạy.

### 4. Duyệt hoặc dừng task

```bash
# Sau khi người dùng/owner duyệt và verify pass.
npm run kaopiz-devkit -- approve --task TASK-123 \
  --summary "Đã hoàn tất tài liệu và kiểm tra liên kết." \
  --improvements "Không có đề xuất workflow mới."

# Dừng session khi task không thể tiếp tục.
npm run kaopiz-devkit -- reject --task TASK-123 -m "Thiếu quyền truy cập Jira"
```

| Lệnh | Tác dụng | Có ghi/đổi trạng thái? |
| --- | --- | --- |
| `approve` | Lưu summary/improvements, verify (mặc định), chuyển sang step kế tiếp hoặc hoàn tất task. | Có, **có**. |
| `reject` | Đánh dấu session `failed` kèm lý do. | Có, **có**. |
| `review comment` | Lưu tín hiệu nhận xét reviewer để audit. | Có. |
| `verify` | Kiểm tra artifacts và command; không tự advance step. | Không, trừ output/log do command con tạo. |

## CLI tools theo nhóm

| Nhóm | Command | Tác dụng cụ thể |
| --- | --- | --- |
| Đồng bộ | `sync --source bundled` | Materialize Hub skill/rule/workflow ra `.cursor/` và `.claude/`. Target hiện được CLI hỗ trợ là `cursor,claude`. |
| Đồng bộ | `pull` | Tải skill chọn lọc từ MCP Hub về `.vibe/skills/`, rồi tạo symlink cho Cursor/Claude. Dùng `--dry-run` trước. |
| Đồng bộ | `push` | Đẩy skill local lên Hub; chỉ dùng khi có quyền quản trị Hub và đã review nội dung. |
| Đồng bộ | `skills list` | Liệt kê skill local và trạng thái symlink. |
| Hooks | `setup-hooks` / `install-hooks` | Cài hooks/permission hoặc pre-push hook; xem `--help` trước vì làm thay đổi config Git/IDE. |
| Orchestration | `start`, `run`, `status`, `step`, `approve`, `reject`, `session` | Quản lý lifecycle của task session như phần trên. |
| Kiểm tra | `validate`, `verify`, `doctor` | Xác thực graph, artifacts/commands và trạng thái DevKit. |
| Observability | `telemetry derive`, `workflow-health`, `skill-health` | Tạo telemetry từ session logs và tính pass@k, dwell/failure metric. |
| Cải tiến | `defect-sync`, `defect-health`, `instinct`, `evolve` | Đồng bộ defect, phân tích chất lượng và tạo đề xuất cải tiến workflow/skill. Chạy `evolve` ở chế độ review/dry-run trước khi áp dụng patch. |
| Lưu trữ | `archive` | Snapshot `PLAN.md`, `REVIEW.md`, `DELIVER.md` và session log theo task. |
| Phân tích | `analyze-bug`, `tokens` | Chuẩn bị bug cho Agent hoặc phân tích token/cost của editor session. |
| Skill | `skill-authoring`, `skills` | Soạn prompt/scan skill và quản lý skill local. Sau khi sửa `.agents/skills/**/SKILL.md`, chạy scan theo rule dự án. |
| Task queue | `devkit-task-runner validate` | Validate cấu trúc `task-queue.json` trước khi chạy hàng loạt. |
| Task queue | `devkit-task-runner run` | Chạy queue: start → step → handler → review → approve. Chỉ dùng với task queue đã review vì có thể advance nhiều task. |
| Task queue | `devkit-task-runner translate` | Dịch markdown bằng Cursor/Claude agent và tạo file song song. Kiểm tra diff trước khi commit. |

Ví dụ an toàn cho đồng bộ skill:

```bash
npm run kaopiz-devkit -- pull --query "database migration" --dry-run
npm run kaopiz-devkit -- skills list
npm run kaopiz-devkit -- sync --source bundled --targets cursor,claude
```

## MCP tools: cách gọi và tác dụng

MCP server `devkit-mcp` expose 14 tools dưới đây. Trong IDE Agent, gọi tool với
JSON object; không chạy các tên tool này trực tiếp trong shell. `project_root`
nên là đường dẫn tuyệt đối của repository khi tool cần đọc profile/session.

### Workflow Vibe

| Tool | Input tối thiểu | Tác dụng cụ thể | Dùng khi |
| --- | --- | --- | --- |
| `get_task_context` | `{ "task_id": "TASK-123" }` | Lấy title, domain, file scope và task context từ Hub; nếu thiếu task ID có thể resolve từ `DEVKIT_TASK_ID` hoặc `.vibe/active.json`. | Bắt đầu task/ingest. |
| `validate_plan` | `{ "plan_content": "# Plan..." }` | Trả `valid`, errors, warnings, recommendations theo checklist Hub. | Trước khi thực thi plan. |
| `fetch_knowhow` | `{ "domain": "payment" }` | Lấy best practices/checklist cho domain. | Khi implement domain nhạy cảm. |
| `verify_compliance` | `{ "artifact": "src/x.ts", "checklist": ["Có test?" ] }` | Đối chiếu artifact với checklist, trả pass/errors/summary. | Sau code/test, trước handoff. |

### Skill và context

| Tool | Input tối thiểu | Tác dụng cụ thể | Dùng khi |
| --- | --- | --- | --- |
| `fetch_skill_authoring_guide` | `{}` | Lấy hướng dẫn viết/chỉnh skill. `part` có thể là `writing-skills`, `hub-skill-layout`, `all`... | Tạo custom skill. |
| `sync_project_skills` | `{ "project_id": "ducnm3" }` | Lấy full payload skills/rules/workflows từ Hub; có thể lọc `profile`, `domains`, `additional_skills`. | Setup/sync toàn bộ skill. |
| `get_skill_set_for_context` | `{ "project_id": "ducnm3", "task_id": "TASK-123" }` | Trả tập skill/rule đã lọc theo task/domain, ít token hơn full sync. | Trước khi chọn skill thực thi. |
| `search_hub_skills` | `{ "query": "database migration", "limit": 10 }` | Tìm skill thật trong Hub, trả rank/id/type/domain. | Chưa biết skill ID. |
| `suggest_common_task_routers` | `{ "query": "small bugfix", "project_root": "/abs/repo" }` | Xếp hạng router như `bug_fix`, `feature_dev`; có thể trả daily workflow preference. | Trước `start --skill`. |
| `resolve_execution_skill` | `{ "skill_id": "code-review", "resolution_strategy": "fallback", "project_root": "/abs/repo" }` | Lấy `SKILL.md`; strategy: `default_only`, `custom_only`, `fallback`, `merge`. | Cần nội dung skill chính xác. |

### Workflow profile

| Tool | Input tối thiểu | Tác dụng cụ thể | Dùng khi |
| --- | --- | --- | --- |
| `get_workflow_manifest` | `{ "workflow_id": "feature_dev", "profile": "backend-node" }` | Lấy workflow manifest, router đã resolve và global rules. | Muốn xem step trước khi bắt đầu. |
| `collect_workflow_profile_context` | `{ "project_root": "/abs/repo", "interview_mode": "brownfield" }` | Scan repo, trả draft profile, evidence/signals, câu hỏi interview và prompt tinh chỉnh. Không tự tin tuyệt đối vào inference. | Setup brownfield/greenfield. |
| `finalize_workflow_profile_interview` | `{ "project_root": "/abs/repo", "interview_choices": { "stack": "node" }, "write_file": true }` | Validate câu trả lời và ghi `devkit.workflow-profile.json`; dùng `force_overwrite` chỉ khi đã review profile cũ. | Sau interview. |

### Cải tiến Hub

| Tool | Input tối thiểu | Tác dụng cụ thể | Dùng khi |
| --- | --- | --- | --- |
| `submit_reflexive_feedback` | `{ "project_id": "ducnm3", "type": "compliance_fail", "artifact": "src/x.ts", "errors": ["Thiếu test"] }` | Lưu feedback pending cho Hub team review; không tự sửa Hub. Type: `compliance_fail`, `review_comment`, `post_mortem`, `user_rule`. | Phát hiện skill/rule cần cải thiện. |

Luồng MCP khuyến nghị:

```text
suggest_common_task_routers
  → get_skill_set_for_context
  → get_task_context
  → fetch_knowhow (nếu có domain)
  → validate_plan
  → verify_compliance
  → submit_reflexive_feedback (nếu phát hiện thiếu sót)
```

## Dùng DevKit với Codex

MCP DevKit đã được thêm global cho Codex bằng stdio server. Kiểm tra từ terminal:

```bash
codex mcp get devkit-mcp
codex mcp list
```

Codex phải mở **phiên mới** sau khi thêm MCP để tools xuất hiện. Entry point của
project là:

```text
node node_modules/@kaopiz/vibe-coding-mcp/dist/index.js
```

Không thêm `.cursor/mcp.json` cho Codex: Codex dùng cấu hình `~/.codex/config.toml`
được quản lý qua `codex mcp add`. Bản DevKit CLI hiện sync skill/rule trực tiếp
cho `cursor,claude`, không phải Codex; vì vậy trong Codex dùng MCP tools và đọc
`.vibe/sessions/<task>/current_instruction.md`, không kỳ vọng slash command
`/devkit-*` của Cursor tự hoạt động.

## Xử lý sự cố

| Hiện tượng | Kiểm tra / cách xử lý |
| --- | --- |
| Không thấy tools trong Codex | Mở phiên Codex mới, chạy `codex mcp get devkit-mcp`, kiểm tra đường dẫn `node_modules/@kaopiz/vibe-coding-mcp/dist/index.js`. |
| MCP lỗi sau khi đổi vị trí repo | Xóa/cập nhật server bằng `codex mcp remove devkit-mcp`, rồi `codex mcp add ...` với đường dẫn tuyệt đối mới. |
| Command thao tác nhầm task | Chạy `session list`, `status --task <id>` rồi luôn truyền `--task <id>`. |
| `verify` fail | Đọc expected artifacts và verify command trong `current_instruction.md`; sửa artifact, không dùng `--no-auto-verify` để bỏ qua gate nếu chưa được owner chấp thuận. |
| `pull`/`sync` không có skill Codex | Đây là giới hạn target của CLI hiện tại; dùng MCP `resolve_execution_skill` hoặc đọc skill dưới `.vibe/skills/`. |
| Không biết command/option của version hiện tại | Chạy `npm run kaopiz-devkit -- <command> --help`. |

## Checklist dùng hằng ngày

1. Chạy `status --task <id>` và đọc `current_instruction.md`.
2. Dùng MCP để lấy context/skill/know-how thay vì đoán workflow.
3. Chỉ chạy `start`, `session activate`, `sync`, `pull`, `approve`, `reject` khi
   hiểu rõ file/state chúng sẽ thay đổi.
4. Chạy `verify` trước handoff; không tự approve thay người duyệt.
5. Sau thay đổi skill, chạy skill scanner theo rule của dự án.
