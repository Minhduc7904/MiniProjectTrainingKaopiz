# 49. Chiến lược nhánh trong 5 ngày

Không cần Git Flow phức tạp (`main` / `develop`). Nhánh tích hợp của intern là
`ducnm3`.

Ngày 1–2: làm trên `ducnm3` (commit trực tiếp hoặc pull request từ
`feature/ducnm3_*`).

Từ **Ngày 3**: mỗi hạng mục một ticket Jira, nhánh `feature/{mã backlog}`.

```text
ducnm3
feature/BLD-124_4
feature/BLD-125_1
```

Chi tiết: [DEV_TASK_GUIDE.md](../guide/DEV_TASK_GUIDE.md) và
[GIT_GUIDE.md](../guide/GIT_GUIDE.md).

- Base luôn là `ducnm3`.
- Tên nhánh = `feature/{mã backlog}` (`feature/BLD-124_4`), không dùng tiêu đề
  Jira đầy đủ.
- Không dùng `feature/ducnm3_*` từ Ngày 3.
- Commit message = nội dung task trên backlog.
- `Chưa tạo` trên cột Ticket = cấm push **code**.
- Markdown tài liệu (`docs/**/*.md`, `AGENTS.md`, `rules/*.md`,
  `.agents/**/*.md`) push thẳng `ducnm3`, không cần ticket/PR.
- Khi user yêu cầu, agent tạo pull request vào `ducnm3` đúng mẫu GIT_GUIDE.

---
# 50. Commit

Từ Ngày 3, message là nội dung task trên backlog (tiếng Việt):

```text
Triển khai base FE

Hoàn thiện Media Service

CSV naive vs stream
```

---
