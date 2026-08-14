# Hướng dẫn quy trình Git

Áp dụng từ **Ngày 3**. Ngày 1–2 đã làm trên `ducnm3` (một số commit trực tiếp,
một số pull request từ `feature/ducnm3_*`). Từ Ngày 3 không dùng
`feature/ducnm3_*`.

Quy trình task đầy đủ: [DEV_TASK_GUIDE.md](DEV_TASK_GUIDE.md).

## Quy ước nhánh

- `ducnm3` là nhánh tích hợp. Mọi pull request merge vào `ducnm3`.
- Mỗi hạng mục = một ticket backlog/Jira = một nhánh
  `feature/{mã backlog}`, ví dụ `feature/BLD-124_4`.
- Không đặt tên nhánh theo tiêu đề Jira
  `【BLD-124_4】 Đối ứng gửi mail khi dừng user tích hợp`.
- Không dùng `feature/ducnm3_<short-description>`.

## Commit

Message phải viết **tiếng Việt**, ngắn gọn nhưng nêu rõ hành động và phạm vi
thay đổi chính. Ưu tiên dùng **nội dung task trên backlog** khi tiêu đề task đã
rõ nghĩa; không viết kiểu `feat:` hay prefix mã nhánh, cũng không dùng message
chung chung như `update`, `fix`, `wip`. Ví dụ backlog “Triển khai base FE” thì:

```bash
git commit -m "Triển khai base FE"
```

Ví dụ rõ phạm vi hơn:

```bash
git commit -m "Chuẩn hóa gán media usage cho notification đơn và batch"
```

## Cổng push

- Cột Ticket trên `docs/plan/` phải có mã Jira trước khi push **code**.
- `Chưa tạo` hoặc trống = **cấm push code**, kể cả khi được yêu cầu push. Commit
  local vẫn được.
- User tạo ticket Jira; agent không tạo Jira. Khi user gửi mã, điền cột Ticket
  rồi mới được push code.
- Chỉ push code khi user yêu cầu rõ **và** cổng ticket đã mở.

## Ngoại lệ tài liệu markdown

Sửa **chỉ** file markdown tài liệu không ảnh hưởng code được commit và **push
thẳng lên `ducnm3`**: không cần mã ticket, không cần nhánh ticket, không bắt
buộc pull request.

Phạm vi ngoại lệ:

- `docs/**/*.md`
- `AGENTS.md`
- `rules/*.md`
- `.agents/**/*.md`

Nếu diff còn `backend/`, `frontend/`, `tests/` hoặc `scripts/` (không phải
markdown tài liệu) thì không dùng ngoại lệ — đi theo nhánh `feature/{mã backlog}`.

```bash
git switch ducnm3
git pull --ff-only origin ducnm3
git add docs AGENTS.md rules .agents
git commit -m "Chuẩn hóa quy trình task từ Ngày 3"
git push origin ducnm3
```

## Bắt đầu một task

```bash
git switch ducnm3
git pull --ff-only origin ducnm3
git switch -c feature/BLD-124_4
```

Giữ mỗi nhánh trong phạm vi một ticket. Không đưa vào sản phẩm được sinh, thông
tin xác thực, `.env` hoặc phần dọn dẹp không liên quan.

## Quy trình hằng ngày

```bash
git status --short
git diff --check
dotnet build backend/Lms.sln -m:1
dotnet test backend/Lms.sln -m:1
git add <changed-files>
git commit -m "Triển khai base FE"
```

Trước khi commit, xác minh các kiểm thử hoặc quy trình chạy cục bộ liên quan.

## Push nhánh task

```bash
git push -u origin feature/BLD-124_4
```

Chỉ chạy khi Ticket trên plan đã có mã và user yêu cầu push.

## Pull request

Khi user yêu cầu, agent **tạo pull request** vào `ducnm3`. Title = nội dung task
trên backlog. Body **bắt buộc** đúng mẫu dưới đây, viết **tiếng Việt**; thuật
ngữ kỹ thuật giữ tiếng Anh (`endpoint`, `payload`, `migration`, `retry`, …).

Không bỏ section. Mục **DB** ghi `Không có` nếu không đổi schema/migration.

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

Target luôn là `ducnm3`. User có thể nhờ agent review PR, hoặc merge luôn.

## Cập nhật nhánh task

Khi `ducnm3` có commit mới, cập nhật nhánh bằng merge không phá hủy:

```bash
git fetch origin
git merge origin/ducnm3
```

Giải quyết xung đột cục bộ, dựng/kiểm thử lại rồi commit kết quả gộp. Không
force-push các nhánh dùng chung.

## Hoàn tất

Sau khi pull request được gộp, chuyển lại về `ducnm3` và xóa nhánh cục bộ đã
gộp:

```bash
git switch ducnm3
git pull --ff-only origin ducnm3
git branch -d feature/BLD-124_4
```
