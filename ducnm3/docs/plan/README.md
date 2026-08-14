# Kế hoạch triển khai năm ngày

Ngày 1–2 giữ lịch sử đã làm trên `ducnm3`. Từ **Ngày 3**, mỗi task trên plan
phải có **Task**, **Est** và **Ticket**. Quy trình làm task:
[DEV_TASK_GUIDE.md](../guide/DEV_TASK_GUIDE.md). Skill agent:
[developer-task](../../.agents/skills/developer-task/).

## Quy ước từ Ngày 3

| Cột | Ý nghĩa |
| --- | --- |
| Task | Một hạng mục = một ticket Jira |
| Est | Estimate giờ thật của Dev; **không** bắt buộc tổng ngày = 8 giờ |
| Ticket | Mã Jira, ví dụ `BLD-124_4`. `Chưa tạo` = **cấm push code** |

User tạo ticket trên Jira; agent không tạo Jira. Khi user gửi mã, agent điền cột
Ticket. Nhánh git: `feature/{mã backlog}` (`feature/BLD-124_4`). Commit message =
nội dung task trên backlog.

Markdown tài liệu (`docs/**/*.md`, `AGENTS.md`, `rules/*.md`, `.agents/**/*.md`)
được push thẳng lên `ducnm3`: không cần ticket, không bắt buộc PR.

Luồng git (code):

1. Có mã ticket trên plan.
2. Tạo nhánh `feature/{mã backlog}`, base `ducnm3`.
3. Push nhánh khi user yêu cầu **và** Ticket khác `Chưa tạo`.
4. Khi user yêu cầu, agent tạo pull request vào `ducnm3` (title = nội dung
   task; body đúng mẫu GIT_GUIDE).
5. User có thể nhờ agent review PR, hoặc merge luôn.

## File theo ngày

- [Ngày 1](day-01-foundation-docker-clean-architecture.md) — đã hoàn thành
- [Ngày 2](day-02-lms-core-minio-n-1.md) — đã hoàn thành
- [Ngày 3](day-03-frontend-media.md) — hoàn thiện Media Service và hai trang FE
- [Ngày 4](day-04-performance-day.md) — áp dụng quy trình ticket
- [Ngày 5](day-05-error-handling-test-slide-demo.md) — áp dụng quy trình ticket
