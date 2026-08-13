# Tổng quan kiến trúc

## Trạng thái

Phần máy chủ sử dụng các vi dịch vụ ASP.NET Core, YARP Gateway, MySQL và MinIO.
Course Service, Student Service, Media Service và Notification Service là các dịch vụ nghiệp vụ. Scheduler là
dịch vụ nền tảng, có API, khung Worker và cơ sở dữ liệu riêng.

Mỗi dịch vụ sở hữu cơ sở dữ liệu riêng. Chỉ Media có quyền truy cập MinIO.
Scheduler lưu `background_jobs` và `background_job_runs` dùng chung; dịch vụ này
không truy vấn cơ sở dữ liệu của dịch vụ khác. Notification Service lưu nội dung gửi hàng
loạt, bản chụp danh sách người nhận và các bộ đếm gửi trong các bảng xử lý hàng
loạt của riêng mình.

Phạm vi Scheduler hiện tại chỉ gồm phần cấu trúc: kiểm tra trạng thái, lược đồ,
khung lưu trữ dữ liệu và khung Worker chưa chạy. Việc phân tích CRON, nhận lượt
chạy để xử lý, thực thi bộ xử lý và gọi liên dịch vụ sẽ được triển khai sau.

Xem `microservices.md`, `clean-architecture.md` và
`../database/lms-data-model.md` để biết chi tiết các ranh giới.
