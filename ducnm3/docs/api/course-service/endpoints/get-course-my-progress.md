# `GET /course/api/courses/{courseId}/my-progress`

Business flow: [`get-course-my-progress.md`](../../../business-flows/course-learning/get-course-my-progress.md).

## Mục đích

Trả tiến độ học của chính Student trong một Course đã ghi danh, phục vụ thẻ Course tại Home.

## Xác thực và phân quyền

- Actor bắt buộc là Student.
- Service xác nhận enrollment theo `courseId` và actor trước khi tính progress.

## Yêu cầu

`courseId` là UUID khác rỗng; không có request body hay query parameter.

## Phản hồi thành công

```http
200 OK
Cache-Control: no-store
```

```json
{
  "data": {
    "courseId": "11111111-1111-1111-1111-111111111111",
    "totalLessons": 4,
    "completedLessons": 2,
    "progressPercent": 50,
    "nextLesson": { "id": "22222222-2222-2222-2222-222222222222", "title": "Dependency injection", "displayOrder": 3 }
  },
  "meta": { "traceId": "01J..." }
}
```

`completedLessons` chỉ tính record có `progressPercent >= 100` và `completedAt` khác `null`. `nextLesson` là Lesson có `displayOrder` nhỏ nhất chưa hoàn thành, hoặc `null`.

## Mã trạng thái HTTP

- `200`: tính progress thành công.
- `400 VALIDATION_FAILED`: `courseId` không hợp lệ.
- `403 STUDENT_NOT_ENROLLED`: Student chưa ghi danh hoặc actor không phải Student.
- `404 COURSE_NOT_FOUND`: Course không còn tồn tại sau khi xác nhận enrollment.

## Điều kiện nghiệp vụ và tác động phụ

Endpoint đọc Lesson và progress của duy nhất actor; không ghi database, không phát event và dùng `Cache-Control: no-store`.

## Đồng bộ artifact

- Postman: `CourseService/GET My course progress`.
- Tests: `StudentLearningHandlersTests`, `StudentLearningEndpointsComponentTests`.
