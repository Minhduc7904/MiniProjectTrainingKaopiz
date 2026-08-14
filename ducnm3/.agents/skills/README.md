# Project Workflow Skills

Mọi skill của project nằm tại `.agents/skills/`. `AGENTS.md` xác định skill
nào phải đọc trước khi code. Cursor và các agent tương thích cũng quét folder
này.

Mỗi workflow skill của MiniProject có:

```text
.agents/skills/<skill>/
├── SKILL.md
├── reference.md
└── template.md
```

- `SKILL.md`: thứ tự workflow và definition of done.
- `reference.md`: quy ước chi tiết của project.
- `template.md`: checklist và skeleton để áp dụng.

Skill cài từ nguồn ngoài (hiện tại: `interface-design`) giữ nguyên file gốc
của nguồn đó, thường chỉ có `SKILL.md`. Vẫn đọc toàn bộ folder đó trước khi
làm UI.

Endpoint skills được tách riêng theo HTTP method/semantics:

- `api-get-detail-endpoint`
- `api-get-list-endpoint`
- `api-post-endpoint`
- `api-put-endpoint`
- `api-patch-endpoint`
- `api-delete-endpoint`

Testing skills không được dùng thay thế lẫn nhau:

- `test-unit`
- `test-component`
- `test-integration`

Schema workflow dùng `database-migration`; release workflow dùng `release`.
Từ Ngày 3, Jira/WBS, estimate, nhánh ticket, cổng push và review PR dùng
`developer-task` — đọc thêm, không thay skill kỹ thuật. Product UI (dashboard,
admin, frontend) dùng `interface-design`.
