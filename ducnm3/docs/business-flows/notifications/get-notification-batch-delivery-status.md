# Lấy trạng thái delivery Notification Batch

## Mục đích

Hiển thị tiến độ gửi dựa trên counter bền vững do Notification Worker cập nhật theo chunk.

## Luồng chính

1. Sau khi snapshot hoàn tất, UI gọi delivery status.
2. Notification API đọc `total_count`, `processed_count`, `success_count`, `failed_count` và timestamp.
3. Application tính remaining, phần trăm và duration.
4. UI polling khi status chưa terminal.
5. Khi delivery terminal, UI mới bắt đầu gọi Media Usage job status.

## Trường hợp lỗi

UUID sai trả `400`; batch không tồn tại trả `404`.

## Dữ liệu thay đổi

Không có; endpoint chỉ đọc Notification DB.
