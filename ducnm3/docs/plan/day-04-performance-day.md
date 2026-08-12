# DAY 4 — Performance Day

Đây là ngày quan trọng nhất.

## Goal

Hoàn thành toàn bộ Before / After benchmark.

---

## Task 1 — CSV

Implement:

```text
/courses/export-naive
/courses/export-stream
```

Test:

```text
10k
100k
```

Record:

```text
Time
Memory
File size
```

---

## Task 2 — Index

Seed:

```text
100k
1M courses
```

Nếu 1M seed quá lâu:

- Giữ script riêng.
- Tạo ít nhất dataset đủ lớn để thấy khác biệt.
- Nhưng mục tiêu cuối vẫn là thử 1M nếu máy cho phép.

Test query:

```text
status + created_at
```

Before:

```text
No index
```

After:

```text
Composite index
```

Run:

```sql
EXPLAIN ANALYZE
```

---

## Task 3 — Pagination

Test:

```text
OFFSET 0
OFFSET 10k
OFFSET 100k
OFFSET 500k
```

Implement cursor:

```text
afterId
```

Compare.

---

## Task 4 — Batch Benchmark

Test:

```text
3k
10k
100k
```

Record:

```text
Time
Memory
Throughput
```

---

## Definition of Done Day 4

- [ ] CSV before/after có số liệu.
- [ ] N+1 before/after có query count.
- [ ] Index before/after có EXPLAIN.
- [ ] Pagination before/after có time.
- [ ] Batch có benchmark.
- [ ] Lưu kết quả vào `docs/benchmark`.

---
