# Hướng dẫn phát triển với MinIO

## Quyền sở hữu và phạm vi hiện tại

Chỉ Media Service được phép truy cập MinIO. `MediaService.Application` sở hữu
các cổng `IStorage` và `IStorageHealthProbe`; `MediaService.Infrastructure`
triển khai cả hai cổng bằng MinIO SDK.

Thiết lập này bao gồm thao tác lưu trữ, kiểm tra hợp lệ, tiêm phụ thuộc,
khởi tạo tài nguyên, kiểm tra trạng thái và kiểm thử. Theo chủ đích, thiết lập
này chưa cung cấp endpoint HTTP tải lên/tải xuống hoặc ghi bản ghi
`media_objects`.

## Bucket và object key

Bên gọi truyền vào loại media, không bao giờ truyền một bucket tải lên tùy ý.

| Loại | Bucket |
| --- | --- |
| `IMAGE` | `images` |
| `VIDEO` | `videos` |
| `DOCUMENT` | `documents` |
| `AUDIO` | `audios` |
| `OTHER` | `other` |

Object key sử dụng ngày upload theo UTC và một UUID được sinh:

```text
yyyy/MM/dd/{uuid}.{extension}
```

Ví dụ, một ảnh có thể được lưu như sau:

```text
bucket: images
object_key: 2026/08/12/619319269e3946dab81657242c11bc86.png
storage_address: images/2026/08/12/619319269e3946dab81657242c11bc86.png
```

Database lưu riêng bucket và object key. Tên bucket và object key là tham chiếu
nội bộ, không được trở thành URL công khai.

## Cấu hình

Sao chép `.env.example` thành `.env` và thay các credential giữ chỗ cục bộ. Các
biến liên quan gồm:

```dotenv
MINIO_ROOT_USER=minio-root-user
MINIO_ROOT_PASSWORD=replace-with-a-long-root-secret
MINIO_APP_ACCESS_KEY=media-storage-app
MINIO_APP_SECRET_KEY=replace-with-a-long-app-secret
MINIO_USE_SSL=false
MINIO_HEALTH_TIMEOUT_SECONDS=3
MINIO_IMAGE_BUCKET=images
MINIO_VIDEO_BUCKET=videos
MINIO_DOCUMENT_BUCKET=documents
MINIO_AUDIO_BUCKET=audios
MINIO_OTHER_BUCKET=other
```

Docker Compose ánh xạ các giá trị này sang cấu hình `Storage__Minio__*` cho
Media Service. Không bao giờ commit `.env` hoặc thông tin xác thực của môi trường sản xuất.

## Khởi tạo tài nguyên và khởi động

Khởi động MinIO và khởi tạo lưu trữ cho Media Service:

```bash
docker compose up -d minio minio-init
```

`minio-init` chờ MinIO đạt trạng thái `healthy`, tạo đủ năm bucket theo cách
idempotent, tạo một người dùng ứng dụng chuyên biệt và gắn policy chỉ giới hạn
trong các bucket đó. Media Service không tạo bucket trong thời gian chạy và sẽ khởi động
thất bại nếu thiếu cấu hình MinIO hoặc cấu hình không hợp lệ.

MinIO API hoạt động tại `http://localhost:9000`; console phát triển hoạt động
tại `http://localhost:9001`.

## Kiểm tra hợp lệ và vận hành

`MinioStorageService` hỗ trợ tải lên và tải xuống dạng luồng, kiểm tra sự tồn
tại, tra cứu metadata và xóa. Trước khi gọi MinIO, service kiểm tra:

- luồng có thể đọc hoặc ghi theo yêu cầu;
- kích thước tải lên khai báo là số dương và khớp với luồng có thể seek;
- phần mở rộng đã được chuẩn hóa và chỉ chứa chữ cái hoặc chữ số;
- loại MIME khớp với `IMAGE`, `VIDEO`, `DOCUMENT`, `AUDIO` hoặc `OTHER`;
- thao tác trên object đã tồn tại nhắm đến một trong các bucket đã cấu hình.

Ngoại lệ của MinIO SDK được bọc thành ngoại lệ lưu trữ do ứng dụng sở hữu.
Thông tin xác thực và tham chiếu object không được ghi vào log.

## Kiểm tra trạng thái

`GET /health` kiểm tra database của Media Service và cả năm bucket MinIO. Bước
thăm dò lưu trữ sử dụng các lời gọi kiểm tra sự tồn tại của bucket với thời gian
chờ ngắn; không bao giờ tải object kiểm thử lên.

Chạy các kiểm thử MinIO cô lập:

```bash
dotnet test backend/Services/Media/MediaService.IntegrationTests/MediaService.IntegrationTests.csproj
```
