# Hướng dẫn Postman collection

Collection chuẩn của project:

```text
postman/MiniProjectKaopiz.postman_collection.json
```

## Quy tắc

- Mỗi endpoint đã implement có một request trong folder service sở hữu.
- Request dùng Gateway public path, không gọi trực tiếp container/service path.
- Tên item theo mẫu `METHOD Resource action`.
- Dùng collection variables cho URL và ID thay đổi:
  `baseUrl`, `studentId`, `ownerId`, `mediaId`.
- Không commit password, token, access key, secret key hoặc URL riêng của máy.
- Request body, query/path parameter, headers và example phải khớp API doc.
- Description của request trỏ tới business-flow tương ứng.
- POST tạo resource có thể lưu ID từ response vào collection variable để request
  tiếp theo sử dụng.

## Workflow sau khi implement endpoint

1. Hoàn tất endpoint và tests.
2. Viết API doc và business-flow 1:1.
3. Thêm/cập nhật Postman request trong cùng change.
4. Kiểm tra request dùng Gateway prefix đúng.
5. Validate JSON:

```bash
jq empty postman/MiniProjectKaopiz.postman_collection.json
```

6. Nếu có thể, smoke-test request trên Docker Compose stack cô lập.

## Endpoint đã backfill

- `GET /student/api/students`.
- `GET /student/api/students/{studentId}`.
- `POST /media/api/media`.
- `POST /media/api/media/usages`.
- `GET /media/api/media/{mediaId}/content`.
