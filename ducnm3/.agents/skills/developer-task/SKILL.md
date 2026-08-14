---
name: developer-task
description: Quy trình một task Jira theo WBS từ Ngày 3 — estimate, design, nhánh feature/{mã backlog}, cổng push, tạo PR vào ducnm3 khi user yêu cầu (mẫu tiếng Việt). Bắt buộc dùng cho Jira/WBS, estimate, ticket branch, push gate hoặc PR từ Ngày 3.
---

# Developer task

Áp dụng từ **Ngày 3**. Ngày 1–2 không dùng skill này.

## Bắt buộc đọc

1. [`docs/guide/DEV_TASK_GUIDE.md`](../../../docs/guide/DEV_TASK_GUIDE.md).
2. [`docs/guide/GIT_GUIDE.md`](../../../docs/guide/GIT_GUIDE.md).
3. [`rules/git.md`](../../../rules/git.md).
4. Task tương ứng trong `docs/plan/` (Ngày 3–5).
5. [`reference.md`](reference.md) và [`template.md`](template.md).

Không code, commit, push hoặc tạo/review PR trước khi đọc đủ các file trên.

Skill HTTP-method, test và migration **không** thay thế skill này. Từ Ngày 3,
một endpoint vẫn đọc skill HTTP-method + test (+ migration nếu đổi schema)
**và** skill này.

## Workflow

1. Xác định đúng một hạng mục trên `docs/plan/` Ngày 3–5. Một hạng mục = một
   ticket Jira; không tách sub-task theo phase.
2. Đọc cột Task, Est, Ticket. Est là giờ thật, không chỉnh cho tổng ngày = 8
   giờ. User tạo Jira; agent không tạo Jira. Khi user gửi mã, điền cột Ticket.
3. **Cổng push code:** nếu Ticket là `Chưa tạo` hoặc trống thì không `git push`
   code, kể cả khi user bảo push. Commit local được.
   **Ngoại lệ docs:** diff chỉ gồm `docs/**/*.md`, `AGENTS.md`, `rules/*.md`,
   `.agents/**/*.md` thì commit và push thẳng `ducnm3` (không cần ticket, không
   bắt buộc PR). Diff có code thì không dùng ngoại lệ.
4. Phase Estimate: đọc spec/plan, điều tra code, điền Est, link spec.
5. Phase Design: Q&A; cập nhật PTYC/PVAH tương đương trong `docs/api/`,
   `docs/business-flows/`, `docs/architecture/`, `docs/database/`.
6. Tạo hoặc chuyển sang `feature/{mã backlog}`, base `ducnm3` (ví dụ
   `feature/BLD-124_4`). Không dùng `feature/ducnm3_*`. Không dùng tiêu đề Jira
   làm tên nhánh.
7. Phase Implement: coding theo skill kỹ thuật tương ứng; self-test; commit
   message viết bằng tiếng Việt, ngắn gọn nhưng nêu rõ hành động và phạm vi thay
   đổi chính; ưu tiên nội dung task backlog khi tiêu đề đã rõ nghĩa. Không dùng
   `feat:` hoặc message chung chung như `update`, `fix`, `wip`.
8. Push nhánh **chỉ khi** Ticket đã có mã **và** user yêu cầu push. Docs-only
   markdown thì push `ducnm3`, không tạo nhánh ticket.
9. Khi user yêu cầu **tạo pull request**: tạo PR target `ducnm3`. Title = nội
   dung task backlog. Body đúng mẫu tiếng Việt trong GIT_GUIDE (Tổng quan,
   Trước chỉnh sửa, Sau chỉnh sửa, Nội dung chỉnh sửa, DB). Thuật ngữ kỹ thuật
   giữ tiếng Anh. Không bỏ section; DB ghi `Không có` nếu không đổi schema.
10. Review PR **chỉ khi** user nhờ. User có thể merge luôn.
11. Phase Thực thi test: support test; fix bug với comment nguyên nhân / phạm vi
    ảnh hưởng / hướng fix.

## Definition of done

- Cột Task / Est / Ticket trên plan khớp ticket Jira (hoặc Ticket vẫn
  `Chưa tạo` nếu user chưa tạo — khi đó chưa push **code**; docs-only markdown
  vẫn push `ducnm3`).
- Design docs trong `docs/` đã cập nhật trước merge PR chính.
- Nhánh `feature/{mã backlog}`; commit message viết bằng tiếng Việt, rõ hành
  động và phạm vi thay đổi; ưu tiên nội dung task backlog khi tiêu đề đã rõ
  nghĩa.
- PR (khi user yêu cầu tạo) đúng mẫu GIT_GUIDE, target `ducnm3`.
- Self-test và test skill tương ứng đã chạy.
- Bug fix (nếu có) ghi đủ ba mục trên Jira.
