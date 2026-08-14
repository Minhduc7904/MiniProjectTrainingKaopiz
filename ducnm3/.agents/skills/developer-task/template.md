# Template developer task

Thay placeholder `<...>`. Dùng checklist này cho đúng một ticket / một hạng mục
trên `docs/plan/`.

## Phiếu task

```text
Ngày plan: 3 | 4 | 5
File plan: docs/plan/day-0<n>-<slug>.md
Task:
Est: <giờ thật, không ép tổng 8 giờ>
Ticket: <BLD-xxx_x | Chưa tạo>
Nhánh: feature/<mã backlog> | chưa tạo nhánh
Spec link:
Q&A open: có | không
```

## Cổng

```text
- [ ] Đã đọc DEV_TASK_GUIDE, GIT_GUIDE, rules/git.md, SKILL/reference/template.
- [ ] Đã đọc skill HTTP-method / test / migration nếu thay đổi thuộc loại đó.
- [ ] Ticket khác `Chưa tạo` trước khi push **code**.
- [ ] Docs-only markdown (`docs/**/*.md`, `AGENTS.md`, `rules/*.md`,
      `.agents/**/*.md`) thì push thẳng `ducnm3`.
- [ ] User đã yêu cầu push trước khi git push (trừ ngoại lệ docs khi user cho
      phép push docs).
- [ ] Tạo PR vào ducnm3 chỉ khi user yêu cầu; body đúng mẫu GIT_GUIDE.
- [ ] Chỉ review PR khi user nhờ.
```

## Estimate

```text
- [ ] Link spec trên Jira / plan.
- [ ] Đã điều tra module/API/file chính.
- [ ] Est là giờ thật, không chỉnh cho tổng ngày = 8 giờ.
```

## Design

```text
- [ ] Q&A critical đã đóng hoặc đã tag BrSE.
- [ ] PTYC: docs/business-flows/ đã cập nhật (mục tiêu, luồng, AC).
- [ ] PVAH: docs/api/, docs/database/, docs/architecture/ đã cập nhật.
- [ ] Design API: đúng một endpoint doc nếu có API mới/sửa.
```

## Implement

```text
git switch ducnm3
git pull --ff-only origin ducnm3
git switch -c feature/<TICKET_KEY>
```

```text
- [ ] Nhánh = feature/{mã backlog}, base ducnm3.
- [ ] Commit message viết bằng tiếng Việt, nêu rõ hành động và phạm vi thay đổi;
      ưu tiên nội dung task backlog khi tiêu đề đã rõ nghĩa; không dùng `feat:`,
      `update`, `fix` hoặc `wip`.
- [ ] Self-test: build/test liên quan pass; Postman case chính nếu có API.
```

## Mẫu pull request (bắt buộc khi user yêu cầu tạo PR)

Title: nội dung task trên backlog.

```text
Mô tả:
Tổng quan:
<tóm tắt mục đích thay đổi và phạm vi>

Trước chỉnh sửa:
<hành vi / API / UI / schema hiện tại>

Sau chỉnh sửa:
<hành vi / API / UI / schema sau khi merge>

Nội dung chỉnh sửa:
<các thay đổi chính: file, endpoint, luồng>

DB (nếu có):
<migration, table, column, index, constraint — hoặc "Không có">
```

Viết tiếng Việt; thuật ngữ kỹ thuật giữ tiếng Anh. Không bỏ section.

## Thực thi test / bug fix

```text
Ticket/bug:
Nguyên nhân:
Phạm vi ảnh hưởng:
Hướng fix:
PR fix:
Verify: Pass | chưa
```

```text
- [ ] Support test đã trả lời SQA nếu được escalate.
- [ ] Comment Jira đủ ba mục khi fix bug.
- [ ] UT Dev bổ sung nếu R1 yêu cầu.
```
