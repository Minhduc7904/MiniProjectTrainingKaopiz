# Function List

| ID | Function | Domain | Status | Runtime evidence | Specification |
| --- | --- | --- | --- | --- | --- |
| F01 | Quản lý Khóa học | Course | Planned | Chưa có Course business endpoint | [Course](course-spec.md#f01--quản-lý-khóa-học) |
| F02 | Quản lý Bài học | Course | Planned | Chưa có Course business endpoint | [Course](course-spec.md#f02--quản-lý-bài-học) |
| F03 | Tra cứu Khóa học | Course | Planned | Chưa có Course query endpoint | [Course](course-spec.md#f03--tra-cứu-khóa-học) |
| F04 | Ghi danh Khóa học | Course | Planned | Chưa có enrollment endpoint | [Course](course-spec.md#f04--ghi-danh-khóa-học) |
| F05 | Cập nhật tiến độ Bài học | Course | Planned | Chưa có progress endpoint | [Course](course-spec.md#f05--cập-nhật-tiến-độ-bài-học) |
| F06 | Xuất danh sách Khóa học CSV | Course | Planned | Chưa có CSV export endpoint | [Course](course-spec.md#f06--xuất-danh-sách-khóa-học-csv) |
| F07 | Tra cứu danh sách Học viên | Student | Existing | `GetStudentsEndpoint` | [Student](student-spec.md#f07--tra-cứu-danh-sách-học-viên) |
| F08 | Tra cứu chi tiết Học viên | Student | Existing | `StudentEndpoints.MapGetStudentById` | [Student](student-spec.md#f08--tra-cứu-chi-tiết-học-viên) |
| F09 | Upload và xử lý Media | Media | Existing | `MapUploadMedia`, Media Worker | [Media](media-spec.md#f09--upload-và-xử-lý-media) |
| F10 | Truy cập Media an toàn | Media | Existing | Content, thumbnail và usage URL endpoints | [Media](media-spec.md#f10--truy-cập-media-an-toàn) |
| F11 | Quản lý Media usage | Media | Existing | `MapCreateMediaUsage`, Media Worker | [Media](media-spec.md#f11--quản-lý-media-usage) |
| F12 | Tạo thông báo đơn | Notification | Existing | `CreateNotificationEndpoint` | [Notification](notification-spec.md#f12--tạo-thông-báo-đơn) |
| F13 | Quản lý hộp thư đến | Notification | Planned | Mới có lấy chi tiết; chưa có inbox/read/read-all | [Notification](notification-spec.md#f13--quản-lý-hộp-thư-đến) |
| F14 | Tạo Notification batch | Notification | Existing | `CreateNotificationBatchEndpoint` | [Notification](notification-spec.md#f14--tạo-notification-batch) |
| F15 | Snapshot recipient batch | Notification | Existing | `SnapshotNotificationBatchConsumer` | [Notification](notification-spec.md#f15--snapshot-recipient-batch) |
| F16 | Dispatch và retry Notification batch | Notification | Existing | `DispatchNotificationBatchConsumer` | [Notification](notification-spec.md#f16--dispatch-và-retry-notification-batch) |
| F17 | Theo dõi Notification batch | Notification | Existing | Batch detail/failed-item endpoints | [Notification](notification-spec.md#f17--theo-dõi-notification-batch) |
| F18 | Chạy dọn dẹp Media theo lịch | Scheduler | Planned | Mới có data boundary và health | [Scheduler](scheduler-spec.md#f18--chạy-dọn-dẹp-media-theo-lịch) |

> [!NOTE]
> Function List là baseline scope. Bất kỳ function mới nào phải được review ở
> mức scope trước khi bổ sung vào Phase 3. Tài liệu/API contract không đủ để
> gắn `Existing`; trạng thái này cần runtime evidence trong source.
