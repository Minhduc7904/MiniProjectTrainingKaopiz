# 46. Data Seed Strategy

Không insert 1M record bằng API.

Tạo seed script.

Ví dụ:

```text
scripts/seed/
```

Có:

```text
seed-10k
seed-100k
seed-1m
```

Seed chỉ những field cần benchmark.

Ví dụ Course:

```text
id
name
status
created_at
```

Không cần generate description dài.

---
