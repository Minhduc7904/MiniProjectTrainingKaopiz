# 54. Kết luận kiến trúc

Kiến trúc cuối cùng nên giữ ở mức:

```text
LMS Mini

├── API Gateway
│
├── Course Service
│   ├── Kiến trúc sạch
│   └── MySQL
│
├── Student Service
│   ├── Kiến trúc sạch
│   └── MySQL
│
├── Media Service
│   ├── Kiến trúc sạch
│   ├── Siêu dữ liệu MySQL + lượt sử dụng media
│   └── MinIO
│
├── Notification Service
│   ├── Kiến trúc sạch
│   └── Các lô thông báo/hộp thư đến trong MySQL
│
└── Scheduler Service
    ├── Kiến trúc sạch
    ├── Khung Worker
    └── Định nghĩa tác vụ/lịch sử lượt chạy trong MySQL
```

Việc thực thi Scheduler và gọi liên dịch vụ chưa được triển khai; Worker hiện
không chạy vòng lặp nền. Scheduler không thay thế dữ liệu lô nghiệp vụ
của Notification Service.

Điểm khác biệt của dự án:

```text
Không phải:
"Em làm được LMS."

Mà là:
"Em có thể chứng minh LMS thay đổi thế nào khi tập dữ liệu tăng,
phát hiện điểm nghẽn bằng số liệu,
sau đó tối ưu đúng nguyên nhân."
```

Đó chính là nội dung nên tập trung trình bày trong buổi trình diễn.
