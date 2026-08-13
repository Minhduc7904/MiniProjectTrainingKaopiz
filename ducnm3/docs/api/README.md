# Tài liệu API

Các hợp đồng API được sắp xếp theo dịch vụ sở hữu. Không ghi tài liệu cho điểm cuối của Media Service trong tài liệu Course Service, kể cả khi Course Service gọi điểm cuối đó.

```text
api/
├── _templates/
│   └── endpoint.md
├── shared/
│   ├── error-format.md
│   └── response-format.md
├── course-service/
│   ├── README.md
│   └── endpoints/
├── student-service/
│   ├── README.md
│   └── endpoints/
├── media-service/
│   ├── README.md
│   └── endpoints/
├── notification-service/
│   ├── README.md
│   └── endpoints/
└── scheduler-service/
    ├── README.md
    └── endpoints/
```

## Hợp đồng bắt buộc của điểm cuối

Mỗi tệp điểm cuối phải bao gồm:

1. Mục đích và dịch vụ sở hữu.
2. Phương thức và đường dẫn HTTP.
3. Điều kiện xác thực và phân quyền.
4. Tham số/thân yêu cầu, kèm ví dụ yêu cầu cụ thể.
5. Phản hồi thành công và ví dụ phản hồi cụ thể.
6. Tất cả mã trạng thái HTTP và mã lỗi dự kiến.
7. Quy tắc kiểm tra hợp lệ và điều kiện tiên quyết về nghiệp vụ.
8. Tác động phụ: bản ghi cơ sở dữ liệu, việc sử dụng phương tiện, tác vụ nền hoặc lời gọi bên ngoài.
9. Hành vi phân trang, tính lũy đẳng, xử lý đồng thời và tương thích khi áp dụng.

Sử dụng `_templates/endpoint.md` cho các tệp điểm cuối mới. Cập nhật hợp đồng của dịch vụ trong cùng thay đổi với phần triển khai.
