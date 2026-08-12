# Business Flows

Mỗi file mô tả một nghiệp vụ độc lập: actor, điều kiện đầu vào, luồng chính, trường hợp lỗi, và dữ liệu thay đổi.

```text
business-flows/
├── course-learning/
│   ├── course-management.md
│   └── enrollment-and-learning-progress.md
├── media/
│   └── media-upload-and-usage.md
└── notifications/
    ├── single-notification-and-inbox.md
    ├── bulk-notification.md
    └── notification-media-content.md
```

- `course-learning/`: quản lý Course/Lesson, ghi danh và tiến độ học.
- `media/`: upload media, tạo usage, và nhúng media vào Markdown.
- `notifications/`: gửi đơn, gửi hàng loạt, inbox, và Markdown có media.
