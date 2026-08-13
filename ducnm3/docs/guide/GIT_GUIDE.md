# Hướng dẫn quy trình Git

## Quy ước nhánh

- `ducnm3` là nhánh tích hợp mặc định.
- Chỉ dùng nhánh tính năng cho một thay đổi tập trung sẽ được rà soát qua yêu cầu gộp (pull request).
- Nhánh tính năng tuân theo mẫu `feature/ducnm3_<short-description>`, ví dụ `feature/ducnm3_health-api-contracts`.

## Bắt đầu một tính năng

```bash
git switch ducnm3
git pull --ff-only origin ducnm3
git switch -c feature/ducnm3_<short-description>
```

Giữ mỗi nhánh trong phạm vi một mục đích. Không đưa vào sản phẩm được sinh, thông tin xác thực, `.env` hoặc phần dọn dẹp không liên quan.

## Quy trình hằng ngày

```bash
git status --short
git diff --check
dotnet build backend/Lms.sln -m:1
dotnet test backend/Lms.sln -m:1
git add <changed-files>
git commit -m "add database-backed service health checks"
```

Dùng chủ đề commit ở thể mệnh lệnh để mô tả kết quả. Trước khi commit, hãy xác minh các kiểm thử hoặc quy trình chạy cục bộ liên quan.

## Mở yêu cầu gộp (pull request)

```bash
git push -u origin feature/ducnm3_<short-description>
```

Tạo yêu cầu gộp (pull request) trong Bitbucket Server và chọn `ducnm3` làm nhánh đích. Kết quả của lệnh push chứa URL tạo yêu cầu gộp trực tiếp cho nhánh nguồn mới.

Yêu cầu gộp cần nêu:

- thay đổi về hành vi hoặc kiến trúc;
- các ảnh hưởng quan trọng đến cấu hình hoặc migration;
- lệnh xác minh chính xác và kết quả;
- công việc tiếp theo được chủ ý loại khỏi phạm vi.

## Cập nhật nhánh tính năng

Khi `ducnm3` có commit mới, cập nhật nhánh tính năng bằng phép gộp không phá hủy:

```bash
git fetch origin
git merge origin/ducnm3
```

Giải quyết xung đột cục bộ, dựng/kiểm thử lại rồi commit kết quả gộp. Không force-push các nhánh dùng chung.

## Hoàn tất

Sau khi yêu cầu gộp (pull request) được gộp, chuyển lại về `ducnm3` và xóa nhánh cục bộ đã gộp:

```bash
git switch ducnm3
git pull --ff-only origin ducnm3
git branch -d feature/ducnm3_<short-description>
```
