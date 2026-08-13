# Các luồng nghiệp vụ

Mỗi endpoint đã triển khai có đúng một flow file: actor, điều kiện đầu vào,
luồng chính, trường hợp lỗi, dữ liệu thay đổi và test mapping. Flow và API doc
phải liên kết qua lại.

```text
business-flows/
├── course-learning/
│   ├── course-management.md
│   └── enrollment-and-learning-progress.md
├── media/
│   ├── post-media.md
│   ├── post-media-usages.md
│   └── get-media-content.md
├── students/
│   └── get-student-by-id.md
├── notifications/
│   ├── single-notification-and-inbox.md
│   ├── bulk-notification.md
│   └── notification-media-content.md
└── scheduler/
    └── media-cleanup.md
```

- `course-learning/`: quản lý Khóa học/Bài học, ghi danh và tiến độ học.
- `media/`: tải media lên, tạo lượt sử dụng và nhúng media vào Markdown.
- `students/`: query và quản lý Học viên.
- `notifications/`: gửi đơn lẻ, gửi hàng loạt, hộp thư đến và Markdown có media.
- `scheduler/`: luồng chạy nền dùng chung dự kiến; hiện chỉ có phần nền tảng, chưa có phần thực thi.

Khi thêm endpoint mới, sao chép
[`_templates/endpoint-flow.md`](_templates/endpoint-flow.md) và đặt tên file
`<method>-<resource>.md`.
