# 54. Kết luận kiến trúc

Kiến trúc cuối cùng nên giữ ở mức:

```text
LMS Mini

├── API Gateway
│
├── Course Service
│   ├── Clean Architecture
│   └── MySQL
│
├── Student Service
│   ├── Clean Architecture
│   └── MySQL
│
├── Media Service
│   ├── Clean Architecture
│   ├── MySQL metadata + media usage
│   └── MinIO
│
├── Notification Service
│   ├── Clean Architecture
│   └── MySQL notification batches/inbox
│
└── Scheduler Service
    ├── Clean Architecture
    ├── Worker skeleton
    └── MySQL job definitions/run history
```

Scheduler execution và cross-service calls chưa được triển khai; Worker hiện
không chạy background loop. Scheduler không thay thế dữ liệu batch nghiệp vụ
của Notification Service.

Điểm khác biệt của project:

```text
Không phải:
"Em làm được LMS."

Mà là:
"Em có thể chứng minh LMS thay đổi thế nào khi dataset tăng,
phát hiện bottleneck bằng số liệu,
sau đó tối ưu đúng nguyên nhân."
```

Đó chính là thứ nên tập trung show trong buổi demo.
