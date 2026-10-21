# `POST /course/api/courses/{courseId}/lessons/{lessonId}/ai-documents/sync` — Đồng bộ tài liệu đính kèm vào hàng chờ trích xuất

Business flow: [`post-course-lesson-ai-documents-sync.md`](../../../business-flows/course-learning/post-course-lesson-ai-documents-sync.md).

## Mục đích

Đưa các tệp đính kèm mới của Lesson vào hàng chờ trích xuất văn bản (worker nền của Course Service xử lý) và trả trạng thái hiện tại của mọi tài liệu. Media Service không phát event khi gắn tệp nên frontend gọi endpoint này khi mở panel AI của Lesson và mỗi khi danh sách tệp đính kèm đổi. Service sở hữu: Course Service. Đường dẫn trực tiếp của service là `POST /api/courses/{courseId}/lessons/{lessonId}/ai-documents/sync`.

## Xác thực và phân quyền

- Xác thực: header `X-Actor-Type` và `X-Actor-Id` (UUID hợp lệ).
- Vai trò: chỉ `ADMIN`. Actor khác trả `403 FORBIDDEN`; thiếu hoặc sai định dạng header trả `400`.
- `courseId`, `lessonId` phải là UUID khác rỗng, sai trả `400 VALIDATION_FAILED`.

## Yêu cầu

### Tham số đường dẫn và truy vấn

- `courseId`, `lessonId` (UUID, bắt buộc).
- `retryFailed` (query, boolean, mặc định `false`): `true` đặt lại các tài liệu `FAILED` về `PENDING` để worker thử lại.

Không có body.

## Phản hồi thành công

Dùng cấu trúc bao trong [`../../shared/response-format.md`](../../shared/response-format.md). Response có `Cache-Control: no-store`.

```http
200 OK
```

```json
{
  "data": {
    "items": [
    {
      "mediaId": "6f1c2b3a-1d4e-4a5b-8c9d-0e1f2a3b4c5d",
      "fileName": "bai-giang-chuong-1.pdf",
      "contentType": "application/pdf",
      "mediaType": "DOCUMENT",
      "status": "PENDING",
      "errorCode": null,
      "errorMessage": null,
      "textLength": 0,
      "updatedAtUtc": "2026-10-21T08:15:30Z"
    }
    ]
  },
  "meta": {
    "traceId": "0HN7K2Q5V8M1A"
  }
}
```

| Field | Ý nghĩa |
| --- | --- |
| `mediaId` | UUID media của tệp đính kèm (cũng là khóa chọn tài liệu khi tạo job). |
| `fileName`, `contentType`, `mediaType` | Thông tin tệp từ Media Service. |
| `status` | `NOT_QUEUED` (chưa xếp hàng; gọi `sync`), `PENDING`, `EXTRACTING`, `EXTRACTED`, `FAILED`, `UNSUPPORTED`. |
| `errorCode`, `errorMessage` | Chỉ có khi `FAILED`/`UNSUPPORTED`. Ví dụ `UNSUPPORTED_FILE_TYPE`, `EMPTY_DOCUMENT`, `INVALID_DOCUMENT`, `MEDIA_TOO_LARGE`, `MEDIA_NOT_FOUND`, `WORKER_INTERRUPTED`, hoặc mã `AI_PROVIDER_*` / `AI_TASK_NOT_CONFIGURED` từ AI Service. |
| `textLength` | Số ký tự văn bản đã trích xuất (`0` khi chưa có). Nội dung văn bản **không** được trả về qua API. |
| `updatedAtUtc` | Lần cập nhật trạng thái gần nhất; `null` khi `NOT_QUEUED`/`UNSUPPORTED` chưa có bản ghi. |

Sắp xếp theo thứ tự hiển thị của tệp đính kèm (`displayOrder`). Không phân trang vì một Lesson chỉ có số ít tài liệu.

## Mã trạng thái HTTP

- `200`: Đã đồng bộ, trả trạng thái mọi tài liệu.
- `400`: `VALIDATION_FAILED` (UUID sai) hoặc thiếu header actor.
- `403`: Actor không phải Admin.
- `404`: `LESSON_NOT_FOUND`.
- `500`: `UNEXPECTED_ERROR` khi Media Service không trả được danh sách tệp đính kèm (cùng hành vi với endpoint chi tiết Lesson).

Lỗi dùng cấu trúc [`../../shared/error-format.md`](../../shared/error-format.md).

## Điều kiện nghiệp vụ và tác động phụ

- Idempotent: tài liệu đã có bản ghi không bị đụng tới (không reset `EXTRACTED`); gọi nhiều lần không tạo trùng nhờ unique `(lesson_id, media_id)`.
- Tệp có định dạng không hỗ trợ (video, audio, `.doc`…) được ghi `UNSUPPORTED` ngay, không vào hàng chờ. Định dạng hỗ trợ: PDF, DOCX, TXT, MD, PNG, JPG, WEBP, GIF.
- Chèn `lesson_attachment_texts` (`PENDING`), đặt `requested_by` là Admin gọi để worker dùng làm actor khi gọi AI Service. Với `retryFailed=true`, đặt lại bản ghi `FAILED` về `PENDING` và xóa lỗi.
- Gọi `GET /api/media/usages/urls` của Media Service để lấy tệp đính kèm hiện tại của Lesson.

## Đồng bộ artifact

- Postman item: `CourseService/POST Sync lesson AI documents` trong `postman/MiniProjectKaopiz.postman_collection.json`.
- Test: [`docs/tests/course-service/unit.md`](../../../tests/course-service/unit.md), [`docs/tests/course-service/component.md`](../../../tests/course-service/component.md).
- Dữ liệu: [`docs/database/course-service/data-model.md`](../../../database/course-service/data-model.md) (`lesson_attachment_texts`).
- Runbook: [`docs/runbooks/lesson-ai-authoring.md`](../../../runbooks/lesson-ai-authoring.md).
