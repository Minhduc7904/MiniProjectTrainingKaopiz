# `GET /course/api/courses/{courseId}/lessons/{lessonId}/ai-documents` — Trạng thái trích xuất tài liệu của Lesson

Business flow: [`get-course-lesson-ai-documents.md`](../../../business-flows/course-learning/get-course-lesson-ai-documents.md).

## Mục đích

Trả danh sách tệp đính kèm của Lesson kèm trạng thái trích xuất văn bản. Frontend poll endpoint này (không toast) trong lúc còn tài liệu `PENDING`/`EXTRACTING`. Endpoint chỉ đọc: tài liệu chưa vào hàng chờ hiện `NOT_QUEUED`, muốn xếp hàng phải gọi [`sync`](post-course-lesson-ai-documents-sync.md). Service sở hữu: Course Service. Đường dẫn trực tiếp của service là `GET /api/courses/{courseId}/lessons/{lessonId}/ai-documents`.

## Xác thực và phân quyền

- Xác thực: header `X-Actor-Type` và `X-Actor-Id` (UUID hợp lệ).
- Vai trò: chỉ `ADMIN`. Actor khác trả `403 FORBIDDEN`; thiếu hoặc sai định dạng header trả `400`.
- `courseId`, `lessonId` phải là UUID khác rỗng, sai trả `400 VALIDATION_FAILED`.

## Yêu cầu

`courseId`, `lessonId` (UUID, bắt buộc). Không có query hay body.

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
      "status": "EXTRACTED",
      "errorCode": null,
      "errorMessage": null,
      "textLength": 18423,
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

Lesson chưa có tệp đính kèm trả `items: []` với `200`.

## Mã trạng thái HTTP

- `200`: Thành công.
- `400`: `VALIDATION_FAILED` (UUID sai) hoặc thiếu header actor.
- `403`: Actor không phải Admin.
- `404`: `LESSON_NOT_FOUND`.
- `500`: `UNEXPECTED_ERROR` khi Media Service không trả được danh sách tệp đính kèm (cùng hành vi với endpoint chi tiết Lesson).

Lỗi dùng cấu trúc [`../../shared/error-format.md`](../../shared/error-format.md).

## Điều kiện nghiệp vụ và tác động phụ

- Không ghi dữ liệu. Đọc `lesson_attachment_texts` (không đọc cột `extracted_text`) và tệp đính kèm từ Media Service (`GET /api/media/usages/urls`).

## Đồng bộ artifact

- Postman item: `CourseService/GET Lesson AI documents` trong `postman/MiniProjectKaopiz.postman_collection.json`.
- Test: [`docs/tests/course-service/unit.md`](../../../tests/course-service/unit.md), [`docs/tests/course-service/component.md`](../../../tests/course-service/component.md).
- Dữ liệu: [`docs/database/course-service/data-model.md`](../../../database/course-service/data-model.md).
