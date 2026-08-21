# Hướng dẫn đo benchmark Batch Notification và CSV

Tài liệu này hướng dẫn chạy `Lms.PerformanceRunner` trên Docker Compose để đo:

- Batch Notification với `3,000`, `10,000` hoặc `100,000` ACTIVE Student.
- CSV Course với `10,000`, `100,000` hoặc `300,000` Course.
- CSV `buffered` Development-only so với endpoint `streaming`.

Đây là benchmark thủ công. Không chạy dataset lớn trong CI và không dùng số liệu
chưa có trong thư mục kết quả làm SLO.

## Điều kiện

Chạy mọi lệnh từ repository root:

```bash
cd /home/ducnm3/duck/2026/month_8/MiniProject/intern_be/ducnm3
```

Cần có:

- Docker Engine và Docker Compose plugin đang chạy.
- `.env` Development hợp lệ.
- .NET SDK `10.0`.
- Gateway tại `http://localhost:5100` hoặc URL được truyền qua `--gateway-url`.
- Đủ CPU, RAM và dung lượng đĩa cho dataset được chọn.

Kiểm tra CLI:

```bash
dotnet run --project backend/Tools/Lms.PerformanceRunner -- --help
```

## Khởi động hệ thống

Khởi động toàn bộ stack ở chế độ nền:

```bash
docker compose up -d --build
```

## Chọn database profile

Runtime mặc định dùng `.env`. Để chạy toàn bộ API và performance trên snapshot
seed, dùng cùng một profile cho mọi lệnh:

```bash
docker compose --env-file .env.seed up -d --build --force-recreate
scripts/database/reset-seed-databases.sh --confirm
scripts/seed/run-development-seed.sh --env-file .env.seed --confirm
```

Performance runner chỉ gọi Gateway, nên đo đúng database mà stack hiện tại đang
dùng. Không chạy lẫn `docker compose` (runtime) với script `--env-file .env.seed`.

Kiểm tra các service:

```bash
docker compose ps
curl -fsS http://localhost:5100/course/health
curl -fsS http://localhost:5100/notification/health
```

Runner cần tìm được container bằng các service name sau:

- Batch memory: `notification-worker`.
- CSV memory: `course-service`.

Runner chạy trên host, gửi HTTP qua Gateway và gọi Docker CLI để đọc
`docker compose ps -q`/`docker stats`; không chạy bên trong container.

## Chuẩn bị dataset

### Cảnh báo reset

Reset xóa toàn bộ năm database Development. Không chạy khi đang cần giữ dữ liệu
local. Script yêu cầu `--confirm` và chỉ cho phép môi trường Development.

```bash
scripts/database/reset-development-databases.sh --confirm
docker compose up -d --build
```

### Seed Batch

Batch benchmark cần đúng số ACTIVE Student. Ví dụ smoke nhỏ:

```bash
scripts/seed/run-development-seed.sh \
  --confirm \
  --students 3000 \
  --courses 1000 \
  --min-lessons 1 \
  --max-lessons 1 \
  --min-courses-per-student 1 \
  --max-courses-per-student 1 \
  --random-seed 20260813
```

Dataset chính dùng lần lượt `10000` và `100000` Student. Student seeder tối đa
`100000`.

### Seed CSV

CSV benchmark cần đúng số Course. Ví dụ smoke nhỏ:

```bash
scripts/seed/run-development-seed.sh \
  --confirm \
  --students 100 \
  --courses 1000 \
  --min-lessons 1 \
  --max-lessons 1 \
  --min-courses-per-student 1 \
  --max-courses-per-student 1 \
  --random-seed 20260813
```

Dataset chính dùng `10000`, `100000` hoặc `300000` Course. Course seeder tối đa
`300000`, nhưng thời gian seed và dung lượng database có thể lớn.

Seeder deterministic khi giữ nguyên toàn bộ option và `--random-seed`. Xem thêm
[hướng dẫn DataSeeder](../guide/DATA_SEED_GUIDE.md).

> Hiện tại PerformanceRunner chưa tự gọi reset/seed hoặc validate row count qua
> API. Hai flag `--prepare-data --confirm-reset` đã có trong CLI contract nhưng
> chưa thay thế các script chuẩn bị dataset. Hãy reset/seed thủ công trước khi đo.

## Chạy smoke benchmark

Smoke dùng dataset nhỏ để kiểm tra Gateway, route, Docker stats và output trước
khi chạy hàng trăm nghìn bản ghi. Vì parser hiện chỉ nhận các kích thước benchmark
đã khóa, smoke dùng kích thước nhỏ nhất được hỗ trợ.

Batch một measured run, không warm-up:

```bash
dotnet run --project backend/Tools/Lms.PerformanceRunner -- \
  batch --users 3000 --warmup 0 --runs 1 \
  --poll-interval-ms 500 \
  --memory-sample-interval-ms 1000 \
  --timeout 00:30:00 \
  --plain
```

CSV cả hai approach:

```bash
dotnet run --project backend/Tools/Lms.PerformanceRunner -- \
  csv --records 10000 --approach both --warmup 0 --runs 1 \
  --memory-sample-interval-ms 1000 \
  --timeout 00:10:00 \
  --plain
```

Nếu smoke thành công, kiểm tra file trong `performance/results/` và hash của
hai approach CSV. Nếu Docker stats không resolve được container, benchmark dừng
và không xuất memory giả.

## Chạy benchmark chính thức

### Batch

Một kích thước:

```bash
dotnet run --project backend/Tools/Lms.PerformanceRunner -- \
  batch --users 10000 --warmup 1 --runs 3 \
  --poll-interval-ms 500 \
  --memory-sample-interval-ms 1000 \
  --timeout 00:30:00 \
  --plain
```

Chạy cả ba kích thước theo thứ tự `3000`, `10000`, `100000`:

```bash
dotnet run --project backend/Tools/Lms.PerformanceRunner -- \
  batch --all --warmup 1 --runs 3 --plain
```

Nên reset và seed lại trước từng kích thước để các bảng batch không tăng dần do
run trước. Ghi lại command seed, random seed, commit và Docker resource limit
cùng result directory.

### CSV

Đo cả buffered và streaming trên cùng dataset:

```bash
dotnet run --project backend/Tools/Lms.PerformanceRunner -- \
  csv --records 100000 --approach both --warmup 1 --runs 3 \
  --memory-sample-interval-ms 1000 \
  --timeout 00:10:00 \
  --plain
```

Chạy cả ba kích thước:

```bash
dotnet run --project backend/Tools/Lms.PerformanceRunner -- \
  csv --all --approach both --warmup 1 --runs 3 --plain
```

`buffered` chỉ được register khi Course Service chạy Development. Không dùng
đường dẫn này để kiểm tra Production; Production phải trả `404`.

`--records` được runner truyền thành query `limit` cho cả hai endpoint CSV, nên
`--records 100000` sẽ nhận đúng tối đa `100000` Course (chưa tính dòng header).
API export vẫn giữ hành vi tương thích: không truyền `limit` thì export toàn bộ
Course phù hợp với `status`.

## Tùy chọn CLI

| Option | Mặc định | Ý nghĩa |
|---|---:|---|
| `--gateway-url` | `http://localhost:5100` | Base URL Gateway |
| `--output` | `performance/results` | Thư mục JSON output |
| `--warmup` | `1` | Số run không đưa vào measured result |
| `--runs` | `3` | Số measured run |
| `--poll-interval-ms` | `500` | Khoảng poll batch |
| `--memory-sample-interval-ms` | `1000` | Khoảng đọc Docker memory, tối thiểu `250 ms` |
| `--timeout` | Batch `30m`, CSV `10m` | Timeout HTTP/scenario |
| `--approach` | `both` | `buffered`, `streaming` hoặc `both` cho CSV |
| `--records` | bắt buộc với lệnh `csv` | Số Course truyền vào API dưới dạng `limit` |
| `--plain` | tắt | Tắt terminal rendering realtime, phù hợp CI/log file |

`--prepare-data` và `--confirm-reset` hiện chỉ được parser kiểm tra; xem cảnh
báo ở phần [Chuẩn bị dataset](#chuẩn-bị-dataset).

## Output và metric

Mặc định output nằm trong `performance/results/` và không được commit. Runner ghi:

- `metadata.json`: timestamp và options.
- `batch-<count>.json`: raw batch results, status, counters, duration,
  throughput, CPU/RAM statistics và resource samples.
- `csv-<count>.json`: raw CSV measurements, bytes, rows, SHA-256,
  headers/TTFB/total time và CPU/RAM statistics.

Batch:

- `PostLatency`: từ trước POST tới khi nhận `202`.
- `TotalDuration`: từ trước POST tới delivery terminal.
- `SnapshotDuration`: thời gian polling-derived sau khi POST accepted.
- `DispatchDuration`: `durationMs` API khi có.
- `EndToEndThroughput`: processed / total seconds.
- Memory: baseline median, peak, average và delta của `notification-worker`.
- CPU: hiện tại, peak và average của `notification-worker`.

CSV:

- `ResponseHeadersTime`: request tới response headers.
- `Ttfb`: request tới byte body đầu tiên.
- `TotalDownloadTime`: request tới đọc hết body.
- `ResponseBytes`, `RowsReceived`, `ContentSha256`.
- Memory: baseline median, peak, average và delta của `course-service`.
- CPU: hiện tại, peak và average của `course-service`.

Khi không dùng `--plain`, runner hiển thị live table trong terminal. CSV cập
nhật byte, row, tốc độ, TTFB và elapsed time sau mỗi chunk. Batch cập nhật phase,
processed/success/failed, throughput và elapsed time sau mỗi lần poll. CPU/RAM là
resource của container Docker được đo bằng `docker stats`; đây không phải CPU/RAM
của toàn bộ máy host. ETA chỉ có thể suy ra khi CSV response có `Content-Length`;
nếu server stream không cung cấp header này, giao diện hiển thị số byte đã nhận.

CSV `both` phải có cùng SHA-256. Hash khác nhau nghĩa là correctness failure,
không được dùng kết quả đó để so sánh hiệu năng.

## Kiểm tra sau khi chạy

```bash
find performance/results -maxdepth 2 -type f -print | sort
jq . performance/results/metadata.json

git diff --check
docker compose ps
```

Không đưa vào result hoặc log các giá trị từ `.env`, connection string, password,
access key hay secret. Chỉ commit curated evidence khi đã được yêu cầu rõ.

## Xử lý lỗi và hủy

- `Gateway` không phản hồi: kiểm tra `docker compose ps` và health endpoint.
- Không tìm thấy `notification-worker` hoặc `course-service`: kiểm tra đúng
  Compose project và service name.
- Dataset sai kích thước: reset database rồi seed lại đúng option.
- CSV hash khác: không dùng summary; kiểm tra filter, dataset và endpoint.
- `Ctrl+C`: runner trả exit code `130`; raw file có thể chỉ chứa các run đã ghi
  trước khi hủy.
- Timeout: giữ lại partial output để điều tra, sau đó sửa môi trường hoặc tăng
  `--timeout` có ghi nhận trong metadata.

Xem thêm [Docker Compose guide](../guide/docker-compose.md),
[DataSeeder guide](../guide/DATA_SEED_GUIDE.md) và
[performance requirements](../development/performance.md).
