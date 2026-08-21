# Liệt kê Media background job

API contract: [`get-media-jobs.md`](../../api/media-service/endpoints/get-media-jobs.md)

## Mục tiêu

Cho ADMIN xem thumbnail derivation, Markdown usage sync, batch usage deletion và
notification usage do Media Worker xử lý; nhận biết tiến độ, retry attempt cùng
lỗi an toàn gần nhất.

## Luồng chính

1. Admin gửi `GET /media/api/media/jobs` với actor header và filter tùy chọn.
2. Media Service xác thực ADMIN, normalize/validate filter và pagination.
3. Repository lọc `media_background_jobs`, count bằng cùng predicate, sắp xếp `updated_at DESC, id DESC` rồi đọc trang offset.
4. API tính remaining/progress từ các counter và trả envelope `200` với `meta.pagination`.

## Trường hợp rỗng và lỗi

- Không có job khớp: `200`, `data: []`.
- Filter/page/correlation ID không hợp lệ: `400 INVALID_MEDIA_JOB_QUERY`.
- Actor không phải ADMIN: `400 INVALID_ACTOR_TYPE`; ADMIN không tồn tại: `404 ACTOR_NOT_FOUND`.

## Dữ liệu thay đổi

Không có. Endpoint không retry, không đổi trạng thái job và không trả payload nội bộ.
