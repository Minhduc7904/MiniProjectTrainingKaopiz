# 49. Branch Strategy 5 ngày

Không cần Git Flow phức tạp.

```text
main
develop
```

Feature branch ngắn:

```text
feature/course
feature/minio
feature/batch
feature/performance
```

Nếu làm một mình và deadline gấp:

```text
main
feature/*
```

là đủ.

---
# 50. Commit gợi ý

```text
chore: initialize microservice solution

feat: add course service clean architecture

feat: add student service

feat: integrate mysql

feat: add minio object storage

feat: add course lesson progress APIs

feat: add notification background job

feat: add retry and idempotency

perf: optimize course n+1 queries

perf: add streaming csv export

perf: add course search index

perf: add cursor pagination

feat: add global error handling

test: add notification batch tests

docs: add benchmark results and demo script
```

---
