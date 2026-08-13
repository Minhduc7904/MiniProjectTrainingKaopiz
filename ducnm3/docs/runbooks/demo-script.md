# 44. Kịch bản trình diễn 30 phút

## 0–3 phút — Vấn đề

Nói:

```text
Mini LMS này không tập trung vào số lượng tính năng.
Mục tiêu là nghiên cứu những vấn đề phía máy chủ xuất hiện khi dữ liệu tăng:
xử lý theo lô, bộ nhớ, truy vấn SQL, chỉ mục và phân trang.
```

---

## 3–6 phút — Kiến trúc

Trình bày:

```text
Gateway
3 vi dịch vụ
3 cơ sở dữ liệu
MinIO
Mạng Docker
```

Giải thích:

- Ranh giới dịch vụ.
- Quyền sở hữu cơ sở dữ liệu.
- Kiến trúc sạch.

---

## 6–9 phút — Docker + MinIO

Trình bày:

```bash
docker compose ps
```

Sau đó:

```text
Tải ảnh thu nhỏ lên
→ Bảng điều khiển MinIO
→ Đối tượng xuất hiện
```

---

## 9–14 phút — Xử lý theo lô

Tạo tác vụ cho:

```text
10 nghìn người dùng
```

Trình bày:

```text
202 Accepted
```

Sau đó:

```text
Nhật ký tiến trình xử lý nền
Số lô
Thử lại
Mục thất bại
```

---

## 14–18 phút — CSV

Trình bày:

```text
100 nghìn bản ghi
```

So sánh:

```text
Nạp toàn bộ
so với
Truyền luồng
```

Trình bày:

```text
thời gian
bộ nhớ
```

---

## 18–21 phút — N+1

Gọi:

```text
/details-naive
```

Trình bày số lượng truy vấn SQL.

Sau đó:

```text
/details-optimized
```

Trình bày số lượng truy vấn đã giảm.

---

## 21–25 phút — Chỉ mục

Chạy:

```sql
EXPLAIN ANALYZE ...
```

Trước khi thêm chỉ mục.

Sau đó trình bày kết quả sau khi thêm chỉ mục.

Nhấn mạnh:

```text
Quét toàn bộ bảng
→ Quét phạm vi chỉ mục
```

---

## 25–27 phút — Phân trang

Trình bày:

```text
OFFSET 500k
```

so với:

```text
con trỏ
```

---

## 27–30 phút — Kết luận

Trình bày bảng:

| Vấn đề | Trước | Sau |
|---|---:|---:|
| Truy vấn N+1 | đo thật | đo thật |
| Bộ nhớ khi xử lý CSV | đo thật | đo thật |
| Độ trễ tìm kiếm | đo thật | đo thật |
| Phân trang | đo thật | đo thật |
| Bộ nhớ khi xử lý theo lô | đo thật | đo thật |

Kết luận bằng sự đánh đổi.

---
