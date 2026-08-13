# Định dạng lỗi dùng chung

Các cấu trúc bao chuẩn cho trường hợp thành công, lỗi, phân trang theo con trỏ và phân trang theo độ lệch được định nghĩa trong [response-format.md](response-format.md).

Sử dụng các trường `error` và `meta.traceId` của định dạng này cho mọi phản hồi lỗi JSON. Lỗi kiểm tra hợp lệ của trường phải nằm trong `error.details`; không thêm trường cấp cao nhất dành riêng cho từng điểm cuối vào phản hồi.
