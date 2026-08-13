# Project Workflow Skills

Skills trong folder này là workflow bắt buộc của project. `AGENTS.md` xác định
skill nào phải đọc trước khi code.

Mỗi skill có:

```text
<skill>/
├── SKILL.md
├── reference.md
└── template.md
```

- `SKILL.md`: thứ tự workflow và definition of done.
- `reference.md`: quy ước chi tiết của project.
- `template.md`: checklist và skeleton để áp dụng.

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
