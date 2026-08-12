# 23. Batch Performance Test

Dataset:

```text
3,000
10,000
100,000
```

Đo:

```text
Execution Time
Peak Memory
Success Count
Failure Count
Throughput
```

Bảng kết quả:

| Records | Strategy | Batch Size | Time | Peak RAM | Throughput |
|---:|---|---:|---:|---:|---:|
| 3k | Load All | - | đo thật | đo thật | đo thật |
| 10k | Load All | - | đo thật | đo thật | đo thật |
| 100k | Load All | - | đo thật | đo thật | đo thật |
| 100k | Batch | 500 | đo thật | đo thật | đo thật |

Không ghi số giả vào slide.

---
# 24. CSV Export — Version 1

Naive:

```text
SELECT ALL
   │
   ▼
List<Course>
   │
   ▼
Build giant CSV string
   │
   ▼
HTTP Response
```

Ví dụ:

```csharp
var courses = await db.Courses
    .AsNoTracking()
    .ToListAsync();

var csv = BuildCsv(courses);
```

Dùng làm benchmark **Before**.

---
# 25. CSV Export — Version 2

Optimized:

```text
Database
   │
   ▼
Read Batch / Stream
   │
   ▼
Write Response Stream
```

Không giữ 100k object và toàn bộ CSV string trong memory.

Các kỹ thuật:

```text
AsNoTracking()
Projection
Batch / Keyset query
StreamWriter
Response.Body
CancellationToken
```

---
# 26. CSV Benchmark

Test:

```text
10k
100k
300k
```

Đo:

```text
Total Time
TTFB nếu đo được
Peak Memory
Output File Size
```

Compare:

```text
SELECT ALL → Build String

vs

Streaming
```

---
# 27. N+1 Use Case

Endpoint:

```http
GET /api/courses/details
```

Response:

```json
{
  "id": 1,
  "name": "C# Backend",
  "lessons": 20,
  "completedStudents": 120
}
```

Bad implementation:

```text
SELECT courses

foreach course
    SELECT lessons
    SELECT progress
```

Ví dụ 100 Course:

```text
1 Course query
100 Lesson queries
100 Progress queries

≈ 201 queries
```

---
# 28. Fix N+1

Sử dụng:

```text
Projection
Join
GroupBy
Include khi phù hợp
```

Ví dụ mục tiêu:

```text
201 queries
     ↓
1–3 queries
```

Bật:

```text
EF Core SQL Logging
```

để demo query count.

---
# 29. Index Use Case

Seed:

```text
10k Course
100k Course
1M Course
```

Query demo:

```sql
SELECT id, name, status, created_at
FROM courses
WHERE status = 'PUBLISHED'
  AND created_at >= @fromDate
ORDER BY created_at DESC
LIMIT 20;
```

Index thử nghiệm:

```sql
CREATE INDEX idx_courses_status_created_at
ON courses(status, created_at);
```

---
# 30. Query Plan

Trước index:

```sql
EXPLAIN ANALYZE
SELECT ...
```

Quan sát:

```text
Table Scan
Rows Examined
Actual Time
```

Sau index:

```text
Index Range Scan
```

So sánh:

```text
Execution Time
Rows Examined
Access Type
Index Used
```

---
# 31. Điều cần giải thích về Index

Không nói:

> Query chậm thì tạo index.

Phải hiểu:

- Selectivity.
- Cardinality.
- Composite index order.
- Covering index.
- Index làm chậm INSERT/UPDATE.
- Index tốn storage.
- Index không phải lúc nào cũng được optimizer chọn.

---
# 32. Pagination — Offset

Endpoint:

```http
GET /api/courses?page=5000&pageSize=20
```

SQL:

```sql
LIMIT 20 OFFSET 100000
```

Test:

```text
OFFSET 0
OFFSET 10,000
OFFSET 100,000
OFFSET 500,000
```

Đo response time.

---
# 33. Pagination — Cursor / Keyset

Endpoint:

```http
GET /api/courses/cursor?afterId=100000&limit=20
```

SQL đơn giản:

```sql
WHERE id > @afterId
ORDER BY id
LIMIT 20
```

Nếu sort theo createdAt:

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

Index:

```sql
(created_at, id)
```

---
# 34. API Benchmark

Tối thiểu benchmark:

```text
GET /courses
GET /courses/details
GET /courses/export
POST /notification-jobs
```

Tool có thể dùng:

```text
curl
hey
wrk
k6
Postman Runner
```

Nếu thiếu thời gian, dùng:

```text
Stopwatch + logs + curl
```

nhưng tốt nhất có một benchmark tool.

---
