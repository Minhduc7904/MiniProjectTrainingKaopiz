# 23. Kiểm thử hiệu năng xử lý theo lô

> Hướng dẫn chạy thực tế: [Performance Benchmark Guide](../performance/README.md).

> [!IMPORTANT]
> Đây là kế hoạch benchmark cho Phase sau, không phải kết quả đã đo. Chỉ ghi số
> liệu thật sau khi chạy trên cùng môi trường, dataset và cấu hình.

Tập dữ liệu:

```text
3,000
10,000
100,000
```

Đo:

```text
Thời gian thực thi
Bộ nhớ cao nhất
Số lượng thành công
Số lượng thất bại
Thông lượng
```

Bảng kết quả:

| Số bản ghi | Chiến lược | Kích thước lô | Thời gian | RAM cao nhất | Thông lượng |
|---:|---|---:|---:|---:|---:|
| 3k | Tải toàn bộ | - | đo thật | đo thật | đo thật |
| 10k | Tải toàn bộ | - | đo thật | đo thật | đo thật |
| 100k | Tải toàn bộ | - | đo thật | đo thật | đo thật |
| 100k | Xử lý theo lô | 500 | đo thật | đo thật | đo thật |

Không ghi số giả vào slide.

---
# 24. Xuất CSV — Phiên bản 1

Cách đơn giản:

```text
SELECT ALL
   │
   ▼
List<Course>
   │
   ▼
Tạo chuỗi CSV khổng lồ
   │
   ▼
Phản hồi HTTP
```

Ví dụ:

```csharp
var courses = await db.Courses
    .AsNoTracking()
    .ToListAsync();

var csv = BuildCsv(courses);
```

Dùng làm benchmark **Trước khi tối ưu**.

---
# 25. Xuất CSV — Phiên bản 2

Đã tối ưu:

```text
Cơ sở dữ liệu
   │
   ▼
Đọc theo lô / truyền phát
   │
   ▼
Ghi luồng phản hồi
```

Không giữ 100k object và toàn bộ chuỗi CSV trong bộ nhớ.

Các kỹ thuật:

```text
AsNoTracking()
Projection
Truy vấn theo lô / keyset
StreamWriter
Response.Body
CancellationToken
```

---
# 26. Benchmark CSV

Kiểm thử:

```text
10k
100k
300k
```

Đo:

```text
Tổng thời gian
TTFB nếu đo được
Bộ nhớ cao nhất
Kích thước tệp đầu ra
```

So sánh:

```text
SELECT ALL → Tạo chuỗi

so với

Truyền phát
```

---
# 27. Trường hợp sử dụng N+1

Điểm cuối:

```http
GET /api/courses/details
```

Phản hồi:

```json
{
  "id": 1,
  "name": "C# Backend",
  "lessons": 20,
  "completedStudents": 120
}
```

Cách triển khai chưa tốt:

```text
SELECT courses

foreach course
    SELECT lessons
    SELECT progress
```

Ví dụ với 100 khóa học:

```text
1 truy vấn khóa học
100 truy vấn bài học
100 truy vấn tiến độ

≈ 201 truy vấn
```

---
# 28. Khắc phục N+1

Sử dụng:

```text
Projection
Join
GroupBy
Include khi phù hợp
```

Ví dụ mục tiêu:

```text
201 truy vấn
     ↓
1–3 truy vấn
```

Bật:

```text
Ghi log SQL EF Core
```

để minh họa số lượng truy vấn.

---
# 29. Trường hợp sử dụng index

Dữ liệu seed:

```text
10k khóa học
100k khóa học
1M khóa học
```

Truy vấn demo:

```sql
SELECT id, name, status, created_at
FROM courses
WHERE status = 'PUBLISHED'
  AND created_at >= @fromDate
ORDER BY created_at DESC
LIMIT 20;
```

Chỉ mục thử nghiệm:

```sql
CREATE INDEX idx_courses_status_created_at
ON courses(status, created_at);
```

---
# 30. Kế hoạch thực thi truy vấn

Trước khi tạo index:

```sql
EXPLAIN ANALYZE
SELECT ...
```

Quan sát:

```text
Quét toàn bộ bảng
Số hàng đã xét
Thời gian thực tế
```

Sau khi tạo index:

```text
Quét phạm vi chỉ mục
```

So sánh:

```text
Thời gian thực thi
Số hàng đã xét
Kiểu truy cập
Chỉ mục được dùng
```

---
# 31. Điều cần giải thích về index

Không nói:

> Truy vấn chậm thì tạo index.

Phải hiểu:

- Độ chọn lọc.
- Lực lượng.
- Thứ tự cột trong index kết hợp.
- Chỉ mục bao phủ.
- Chỉ mục làm chậm INSERT/UPDATE.
- Chỉ mục tốn dung lượng lưu trữ.
- Chỉ mục không phải lúc nào cũng được trình tối ưu chọn.

---
# 32. Phân trang — Offset

Điểm cuối:

```http
GET /api/courses?page=5000&pageSize=20
```

SQL:

```sql
LIMIT 20 OFFSET 100000
```

Kiểm thử:

```text
OFFSET 0
OFFSET 10,000
OFFSET 100,000
OFFSET 500,000
```

Đo thời gian phản hồi.

---
# 33. Phân trang — Cursor / Keyset

Điểm cuối:

```http
GET /api/courses/cursor?afterId=100000&limit=20
```

SQL đơn giản:

```sql
WHERE id > @afterId
ORDER BY id
LIMIT 20
```

Nếu sắp xếp theo createdAt:

```sql
WHERE
    created_at < @cursorCreatedAt
 OR (
    created_at = @cursorCreatedAt
    AND id < @cursorId
 )
ORDER BY created_at DESC, id DESC
LIMIT 20
```

Chỉ mục:

```sql
(created_at, id)
```

---
# 34. Benchmark API

Các endpoint cần benchmark tối thiểu:

```text
GET /courses
GET /courses/details
GET /courses/export
GET /courses/{courseId}/details
POST /notification-batches
```

Với `GET /courses/{courseId}/details`, đo riêng hai repository path bằng cùng
`courseId`: path production thực hiện tối đa ba query (Course, Lessons,
Progresses theo `IN`); path benchmark thực hiện thêm một query Progress cho mỗi
Lesson. Không dùng Lazy Loading và không dùng path N+1 trong HTTP endpoint.

Công cụ có thể sử dụng:

```text
curl
hey
wrk
k6
Postman Runner
```

Nếu thiếu thời gian, dùng:

```text
Stopwatch + nhật ký + curl
```

nhưng tốt nhất nên có một công cụ benchmark.

---
