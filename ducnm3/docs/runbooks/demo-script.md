# 44. Demo Script 30 phút

## 0–3 phút — Problem

Nói:

```text
Mini LMS này không tập trung vào số lượng feature.
Mục tiêu là nghiên cứu những vấn đề backend xuất hiện khi dữ liệu tăng:
batch, memory, SQL query, index và pagination.
```

---

## 3–6 phút — Architecture

Show:

```text
Gateway
3 Microservices
3 DB
MinIO
Docker Network
```

Giải thích:

- Service boundary.
- DB ownership.
- Clean Architecture.

---

## 6–9 phút — Docker + MinIO

Show:

```bash
docker compose ps
```

Sau đó:

```text
Upload thumbnail
→ MinIO Console
→ Object xuất hiện
```

---

## 9–14 phút — Batch

Create job cho:

```text
10k users
```

Show:

```text
202 Accepted
```

Sau đó:

```text
Worker logs
Batch number
Retry
Failed item
```

---

## 14–18 phút — CSV

Show:

```text
100k records
```

Compare:

```text
Load All
vs
Streaming
```

Show:

```text
time
memory
```

---

## 18–21 phút — N+1

Call:

```text
/details-naive
```

Show SQL query count.

Sau đó:

```text
/details-optimized
```

Show query count giảm.

---

## 21–25 phút — Index

Run:

```sql
EXPLAIN ANALYZE ...
```

Before index.

Sau đó show After index.

Highlight:

```text
Table Scan
→ Index Range Scan
```

---

## 25–27 phút — Pagination

Show:

```text
OFFSET 500k
```

vs:

```text
cursor
```

---

## 27–30 phút — Conclusion

Show bảng:

| Problem | Before | After |
|---|---:|---:|
| N+1 queries | đo thật | đo thật |
| CSV memory | đo thật | đo thật |
| Search latency | đo thật | đo thật |
| Pagination | đo thật | đo thật |
| Batch memory | đo thật | đo thật |

Kết luận bằng trade-off.

---
