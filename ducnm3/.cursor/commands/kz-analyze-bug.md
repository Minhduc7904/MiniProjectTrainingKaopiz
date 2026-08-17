# kz-analyze-bug

**UC10 — Phân tích bug sau khi sửa xong:** phân loại workflow phase, technical cause, human factor, config attribution; lưu record local để `--report --hotspots` / `--push`.

**In Cursor:** slash **`/kz-analyze-bug`** (alias **`/analyze-bug`**).

---

## Input

| Tham số | Bắt buộc | Mô tả |
| ------- | --------- | ----- |
| `<BUG-ID>` | **Có** | Id ticket bug (Jira key, vd. `BUG-123`, `PROJ-456`). |
| `[SOURCE]` | Không | Chỉ **`jira`** hoặc **`manual`** (xem bảng dưới). |

**SOURCE** (optional — truyền sang `@analyze-bug`):

| Giá trị | Ý nghĩa |
| ------- | -------- |
| `jira` | Fetch ticket qua MCP Atlassian (`jira_get_issue`) |
| `manual` | Không fetch — user cung cấp title/description bug trong chat |

Không truyền `SOURCE` → mặc định **`jira`** (fetch MCP). Nếu MCP không khả dụng → hỏi user chọn `manual` và bổ sung mô tả.

**Ví dụ:**

```text
/kz-analyze-bug BUG-123
/kz-analyze-bug BUG-123 jira
/kz-analyze-bug BUG-123 manual
```

Với `manual`, agent hỏi thêm summary/description trước khi phân tích.

---

## Agent — required

1. **Parse input:**
   - `<BUG-ID>` — Jira-style key (`^[A-Z][A-Z0-9_]+-\d+$`) hoặc id user cung cấp khi `manual`.
   - `[SOURCE]` — chỉ `jira` hoặc `manual`; giá trị khác → hỏi lại user.
2. **Nếu thiếu `<BUG-ID>`** → **dừng**, hỏi: *"Bug id cần phân tích là gì?"*
3. **Đọc và thực thi skill** `analyze-bug`:
   - `@analyze-bug <BUG-ID>` với `SOURCE` (`jira` | `manual`).
   - `manual`: thu thập mô tả bug từ user, không gọi Jira MCP.
   - 2 gate, lưu record, `analyze-bug <BUG-ID> --save-record`.
4. Gợi ý sau lưu: `--push`, (tuỳ chọn) `--report --hotspots`.

---

## YOU (AI) playbook

- **Do:** validate `BUG-ID`; chỉ accept `SOURCE` = `jira` | `manual`; `--save-record` sau khi ghi JSON.
- **Do not:** dùng github/bitbucket/URL làm SOURCE; bịa nội dung khi `manual` mà user chưa cung cấp mô tả.

---

## Reference

- Skill: **`analyze-bug`** (`@analyze-bug`)
- Sửa bug: **`/kz-bugfix`**
- Push: `npm run kaopiz-devkit -- analyze-bug --push`

Sync → **`.cursor/commands/kz-analyze-bug.md`**
