# kz-analyze-token

**Phân tích token usage + ước tính chi phí USD** cho một phiên làm việc của editor (opencode | cursor | claude). Báo cáo gồm: breakdown theo category, top tools, cache efficiency, mục **KAOPIZ DEVKIT COST**, và rollup chi phí subagent.

**In Cursor / Claude:** slash **`/kz-analyze-token`** (alias **`/analyze-token`**, **`/tokens`**).

---

## Input

| Tham số | Bắt buộc | Mô tả |
| ------- | --------- | ----- |
| `[EDITOR]` | Không | `opencode` (mặc định) \| `cursor` \| `claude` — đọc session từ editor nào. |
| `[SESSION_ID]` | Không | Id session cụ thể. Bỏ trống → lấy session mới nhất của thư mục hiện tại. |

**Ví dụ:**

```text
/kz-analyze-token
/kz-analyze-token claude
/kz-analyze-token claude 0d017c3b-4677-415c-879b-af1e29efd5a5
/kz-analyze-token cursor
```

---

## Agent — required

1. **Xác định editor** đang chạy phiên này:
   - Nếu user đang chạy trong **Claude Code** → `--editor claude`.
   - Nếu trong **Cursor** → `--editor cursor`.
   - Nếu trong **opencode** → `--editor opencode` (mặc định).
   - User có thể override bằng tham số `[EDITOR]`.
2. **Chạy CLI** (đọc DB/JSONL session cục bộ, read-only):
   ```bash
   npm run kaopiz-devkit -- tokens --editor <EDITOR> [--session <SESSION_ID>] -p .
   ```
   - Bỏ `--session` để auto-resolve session mới nhất cho `-p .`.
   - Thêm `--json` nếu cần output máy đọc.
   - Thêm `--refresh-pricing` nếu model chưa có trong bảng giá (fetch models.dev).
3. **Hiển thị nguyên văn** report cho user. KHÔNG tóm tắt/cắt xén — report đã định dạng sẵn.

---

## YOU (AI) playbook

- **Do:** chọn `--editor` khớp môi trường đang chạy; chạy CLI rồi in nguyên report.
- **Do not:** tự bịa số token/chi phí; không gọi tool khác để "ước lượng" — chỉ dùng output của `kaopiz-devkit tokens`.

---

## Notes

- **opencode / cursor** cần optional dep `better-sqlite3` (đọc SQLite). **claude** đọc JSONL, không cần driver.
- Mục **KAOPIZ DEVKIT COST** chỉ hiện khi session có dùng DevKit (skill/command/CLI/MCP tool). Session thuần Claude/Cursor không qua DevKit sẽ không có mục này.
- Model lạ → ước tính theo giá `claude-sonnet-4-6` (ghi rõ `~estimate` trong report).

---

## Reference

- CLI: **`kaopiz-devkit tokens`** (`--editor`, `--session`, `--json`, `--no-subagents`, `--refresh-pricing`)
- MCP tool tương đương: **`analyze_token_usage`**

Sync → **`.claude/commands/kz-analyze-token.md`** / **`.claude/commands/kz-analyze-token.md`**
