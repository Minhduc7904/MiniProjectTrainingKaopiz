# Lấy trạng thái đăng ký Media Usage từ Markdown

## Mục đích

Cho UI theo dõi phần việc background do Media Service sở hữu sau khi Notification delivery đã terminal.

## Luồng chính

1. Khi tạo Notification Batch, Notification Service gửi `StartNotificationMediaUsageJobV1` qua Outbox.
2. Mỗi chunk gửi thành công phát `RegisterNotificationMediaUsageBatchV1(jobId, ...)`; Media Worker tạo usage và tăng counter.
3. Khi delivery terminal, Notification Service phát `CompleteNotificationMediaUsageJobV1(jobId, expectedUsageCount)`.
4. Media job chỉ terminal khi tổng successful/failed usage đạt expected count, kể cả completion marker tới trước chunk cuối.
5. UI chỉ bắt đầu polling Media API sau khi delivery terminal.

## Trường hợp lỗi

- `404` khi job chưa được Media Worker tiếp nhận hoặc không tồn tại.
- Command hết retry được fault consumer ghi vào `failedUsageCount` bằng thông báo an toàn.

## Dữ liệu thay đổi

GET không đổi dữ liệu. Worker ghi `notification_media_usage_jobs` và `media_usages` trong Media DB; không ghi Notification DB.
