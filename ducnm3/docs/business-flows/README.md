# Các luồng nghiệp vụ

Mỗi file mô tả một nghiệp vụ độc lập: tác nhân, điều kiện đầu vào, luồng chính, trường hợp lỗi và dữ liệu thay đổi.

```text
business-flows/
├── course-learning/
│   ├── course-management.md
│   └── enrollment-and-learning-progress.md
├── media/
│   └── media-upload-and-usage.md
├── notifications/
    ├── single-notification-and-inbox.md
    ├── bulk-notification.md
    └── notification-media-content.md
└── scheduler/
    └── media-cleanup.md
```

- `course-learning/`: quản lý Khóa học/Bài học, ghi danh và tiến độ học.
- `media/`: tải media lên, tạo lượt sử dụng và nhúng media vào Markdown.
- `notifications/`: gửi đơn lẻ, gửi hàng loạt, hộp thư đến và Markdown có media.
- `scheduler/`: luồng chạy nền dùng chung dự kiến; hiện chỉ có phần nền tảng, chưa có phần thực thi.
