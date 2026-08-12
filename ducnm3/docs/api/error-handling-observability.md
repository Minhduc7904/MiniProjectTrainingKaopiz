# 35. Error Handling

Tạo global middleware:

```text
ExceptionHandlingMiddleware
```

Mapping:

```text
ValidationException
    → 400

NotFoundException
    → 404

ConflictException
    → 409

Unhandled Exception
    → 500
```

Response format:

```json
{
  "code": "COURSE_NOT_FOUND",
  "message": "Course not found",
  "traceId": "..."
}
```

Không expose:

- Stack trace.
- Connection string.
- SQL nội bộ.
- Secret.

---
# 36. Logging / Observability

Sử dụng Serilog.

Nên log:

```text
Request
Response Status
Elapsed Time
CorrelationId

Batch Job Start
Batch Number
Batch Size
Success Count
Failure Count
Retry
Elapsed Time

Database slow operation
Unhandled exception
```

Ví dụ:

```text
JobId=123
Batch=15
Records=500
Success=493
Failed=7
ElapsedMs=836
```

---
