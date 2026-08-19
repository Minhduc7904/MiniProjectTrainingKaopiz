# ERBUL26-3050 — Performance Benchmark Tool cho Batch Notification và CSV

> Trạng thái: Draft đã khóa scope, chờ validate/approval trước khi implement.
> Nhánh thực thi: feature/ERBUL26-3050.
> Target merge: ducnm3.

## 1. Task và mục tiêu

Xây dựng một công cụ benchmark có thể chạy lặp lại trên Docker Compose hiện tại
để cung cấp raw evidence và summary cho hai bài toán:

1. Bulk Notification/Batch Processing với 3.000, 10.000 và 100.000 recipient.
2. CSV Export với 10.000, 100.000 và 300.000 Course, so sánh baseline buffered
   với endpoint streaming/chunking hiện có.

Tool phải đo toàn bộ lifecycle thật, memory thật của container, lưu kết quả có
metadata và không thay đổi production behavior chỉ để tạo số liệu benchmark.

## 2. Nguồn yêu cầu và source of truth

- NFR: docs/project/3/non-functional-requirements.md.
- Kiến trúc backend: docs/architecture/backend/overview.md.
- Batch flow: docs/business-flows/notifications/bulk-notification.md.
- Performance requirements: docs/development/performance.md.
- CSV contract: docs/api/course-service/endpoints/get-courses-export.md.
- Seeder: backend/Tools/Lms.DataSeeder/.
- Batch API/worker: backend/Services/Notification/.
- Streaming CSV: backend/Services/Course/.
- Local runtime: docker-compose.yml và scripts/database/, scripts/seed/.

Code/domain constants hiện tại là source of truth cho route, response envelope,
status, batch size, concurrency và terminal state. Không copy magic string vào
runner nếu project đã có contract tương ứng.

## 3. Quyết định đã xác nhận

- ERBUL26-3050 sở hữu cả Batch Notification benchmark và CSV benchmark.
- Course seeder phải hỗ trợ tối đa 300.000 Course. Default vẫn giữ 100.000 để
  tránh một lệnh seed không tham số vô tình tạo dataset quá lớn.
- Student seeder giữ giới hạn 100.000 vì batch requirement lớn nhất là 100.000.
- Buffered CSV là baseline xấu có chủ đích, không phải production
  implementation.
- Buffered endpoint chỉ được register khi Environment.IsDevelopment().
- Production không register route này; gọi route ở Production phải nhận 404.
- Buffered và streaming dùng cùng filter, stable order, columns, CSV encoding,
  escaping và dataset.
- Dataset benchmark được reset và seed deterministic giữa các scenario.
- Mỗi result phải lưu seed options, dataset size, runtime config và Git/Docker
  metadata để các lần đo có thể so sánh công bằng.
- Heavy benchmark là manual/explicit; không chạy 100k/300k trong CI mặc định.

## 4. Phạm vi

### 4.1 In scope

- Project backend/Tools/Lms.PerformanceRunner và unit test tương ứng.
- CLI batch, csv và chế độ chạy tập dataset định sẵn.
- Environment/dataset validation và opt-in reset/seed deterministic.
- Docker Compose container memory sampling.
- Full lifecycle polling của Notification batch.
- Buffered Development-only Course CSV endpoint.
- Streaming client đọc body incremental bằng ResponseHeadersRead.
- Raw result, memory samples, summary CSV và metadata JSON.
- Terminal progress bằng Spectre.Console, có plain-output fallback.
- Unit/component/integration tests phù hợp và tài liệu chạy benchmark.
- Mở giới hạn Course seed từ 100.000 lên 300.000 cùng boundary test.

### 4.2 Out of scope

- Không thay đổi batch production algorithm, retry policy, lease hoặc schema.
- Không thay streaming endpoint production bằng buffered implementation.
- Không expose benchmark endpoint ở Staging/Production.
- Không thêm authentication/authorization mới cho toàn hệ thống.
- Không dùng GC.GetTotalMemory của runner để đại diện memory service.
- Không tự động chạy benchmark nặng trong build/CI.
- Không công bố SLO hoặc số liệu chưa chạy thật.
- Không benchmark N+1, index, pagination và API load ngoài hai scenario đã khóa.
- Không thêm dashboard web hoặc hệ thống metrics dài hạn.

## 5. Acceptance criteria

### AC-BATCH

- AC-BATCH-01: chạy được 3k, 10k và 100k recipient; POST latency không bị coi là
  total execution time.
- AC-BATCH-02: runner poll tới terminal state theo code hiện tại và ghi total
  time, processed/success/failed, throughput, final status.
- AC-BATCH-03: peak/baseline/average/delta memory lấy từ notification-worker
  container thật.
- AC-BATCH-04: dataset ACTIVE Student đúng requested size và được seed
  deterministic.
- AC-BATCH-05: snapshot duration ghi rõ là polling-derived, có sai số tối đa
  bằng poll interval; dispatch duration dùng timestamp API khi có.

### AC-CSV

- AC-CSV-01: benchmark 10k, 100k và 300k Course.
- AC-CSV-02: buffered và streaming trả cùng logical rows, columns, header,
  encoding và filter.
- AC-CSV-03: đo ResponseHeadersTime, TTFB, TotalDownloadTime, response bytes,
  row count và Course Service container memory.
- AC-CSV-04: buffered endpoint chỉ tồn tại ở Development; Production trả 404.
- AC-CSV-05: streaming client không buffer toàn response trước khi đo TTFB.

### AC-EVIDENCE

- AC-EVIDENCE-01: warm-up không đi vào summary; mặc định 1 warm-up và 3 measured
  runs.
- AC-EVIDENCE-02: mỗi run lưu raw CSV; mỗi scenario lưu memory samples; mỗi
  execution lưu metadata JSON và summary CSV.
- AC-EVIDENCE-03: timeout/cancel/error vẫn flush evidence đã thu được và không
  để orphan docker process.
- AC-EVIDENCE-04: result không chứa password, connection string hoặc secret.

## 6. Kiến trúc

~~~text
CLI
 ├── EnvironmentValidator
 ├── DatasetCoordinator
 ├── BatchNotificationScenario
 │    ├── NotificationBenchmarkClient → Gateway
 │    └── ContainerStatsMonitor → notification-worker
 ├── CsvExportScenario
 │    ├── CourseBenchmarkClient → Gateway
 │    └── ContainerStatsMonitor → course-service
 └── Reporting
      ├── TerminalReporter
      ├── RawResultWriter
      ├── SummaryCalculator
      └── MetadataWriter
~~~

Separation bắt buộc:

~~~text
CLI → Configuration → Scenario → Monitoring → Result → Reporting
~~~

Runner chạy trên host để dùng dotnet, HTTP Gateway, docker compose và các script
reset/seed hiện có. Runner không kết nối trực tiếp vào database để đọc metric
nghiệp vụ.

## 7. CLI contract

### 7.1 Batch

~~~bash
dotnet run --project backend/Tools/Lms.PerformanceRunner -- \
  batch --users 3000 --warmup 1 --runs 3

dotnet run --project backend/Tools/Lms.PerformanceRunner -- \
  batch --all --warmup 1 --runs 3
~~~

Batch --all chạy theo thứ tự 3.000, 10.000, 100.000. Có thể override order bằng
việc gọi từng command, nhưng metadata phải ghi order thực tế.

### 7.2 CSV

~~~bash
dotnet run --project backend/Tools/Lms.PerformanceRunner -- \
  csv --records 100000 --approach both --warmup 1 --runs 3

dotnet run --project backend/Tools/Lms.PerformanceRunner -- \
  csv --all --approach both --warmup 1 --runs 3
~~~

CSV --all chạy 10.000, 100.000, 300.000. Approach nhận buffered, streaming hoặc
both; summary so sánh chỉ sinh khi chạy both.

### 7.3 Option chung

~~~text
--gateway-url <url>             default http://localhost:5100
--output <path>                 default performance/results
--warmup <n>                    default 1
--runs <n>                      default 3
--poll-interval-ms <n>          default 500
--memory-sample-interval-ms <n> default 1000
--timeout <duration>            scenario-specific safe default
--prepare-data                  cho phép runner reset/seed
--confirm-reset                 bắt buộc cùng --prepare-data
--plain                         tắt live ANSI rendering
~~~

--prepare-data thiếu --confirm-reset phải fail trước mọi mutation. Không truyền
--prepare-data thì runner chỉ validate dataset và fail với hướng dẫn cụ thể nếu
dataset không khớp.

## 8. Dataset lifecycle và tính công bằng

### 8.1 Nguyên tắc reset

- Chỉ reset Development database.
- Tận dụng scripts/database/reset-development-databases.sh và
  scripts/seed/run-development-seed.sh; không tạo cơ chế truncate bí mật trong
  runner.
- Runner gọi script bằng path resolve từ repository root, capture exit code và
  dừng scenario nếu reset/seed thất bại.
- Reset là opt-in hai lớp: --prepare-data và --confirm-reset.
- Metadata lưu command/options đã dùng nhưng redact secret.

### 8.2 Batch dataset

Mỗi warm-up hoặc measured run được coi là một scenario instance độc lập:

1. Reset Development databases.
2. Seed đúng N ACTIVE Student với cùng random seed; Course count tối thiểu phục
   vụ seeder nhưng không tham gia benchmark.
3. Khởi động/đợi Gateway, Student, Notification, Worker, RabbitMQ và MySQL
   healthy.
4. Validate Student API report đúng N ACTIVE Student.
5. Tạo batch với requestedCount=N.
6. Chạy benchmark và lưu result.

Cách này tránh notification_batches/items/notifications/outbox từ run trước làm
thay đổi query cost của run sau. Chi phí reset/seed không được tính vào benchmark
duration nhưng được ghi riêng trong metadata/preparation log.

### 8.3 CSV dataset

Với mỗi record count 10k/100k/300k:

1. Reset Development databases một lần.
2. Seed đúng N Course bằng random seed cố định; Student count tối thiểu.
3. Validate Course count bằng API/seed summary trước khi đo.
4. Warm up cả buffered và streaming.
5. Chạy measured pair trên cùng dataset. Đảo thứ tự approach theo run
   (buffered-first rồi streaming-first) để giảm cache-order bias.
6. Không reset giữa hai approach trong cùng pair.

Buffered và streaming phải dùng cùng status filter, stable order và export
columns. Runner kiểm tra header, số dòng và hash SHA-256 của nội dung tải về;
hash khác nhau làm run fail correctness và không đưa vào performance summary.

## 9. Buffered endpoint Development-only

Route nội bộ đề xuất:

~~~http
GET /api/performance/courses/export-buffered?status=<optional>
~~~

Public route qua Gateway:

~~~http
GET /course/api/performance/courses/export-buffered?status=<optional>
~~~

Ràng buộc:

- Program.cs chỉ gọi MapBufferedCourseExportBenchmark khi
  app.Environment.IsDevelopment().
- Không có registration ở Production; component test chạy Production xác nhận
  404.
- Endpoint dùng ExportCoursesQuery.Create, cùng projection, filter, stable
  ordering và CourseExportRow như streaming.
- Repository baseline materialize toàn bộ CourseExportRow.
- Handler baseline tạo toàn bộ CSV bytes trong memory rồi mới ghi response.
- Content-Type, Content-Disposition, UTF-8 BOM, header và escaping giống
  streaming endpoint.
- Tên namespace/route/documentation phải thể hiện rõ Performance/Buffered và
  intentional baseline; không đặt nó cạnh production endpoint như lựa chọn mặc
  định.
- Không thêm endpoint vào public API documentation dành cho Production. Tài
  liệu performance phải nói rõ Development-only.

## 10. Định nghĩa metrics

### 10.1 Batch

| Metric | Cách tính |
| --- | --- |
| PostLatencyMs | Từ trước POST đến lúc nhận 202 và parse batchId |
| TotalDurationMs | Từ trước POST đến lần poll đầu thấy terminal state |
| SnapshotDurationMs | Từ POST accepted đến lần poll đầu thấy snapshot completed; polling-derived |
| SnapshotTimingErrorMs | Bằng PollIntervalMs; limitation bắt buộc ghi |
| DispatchDurationMs | Ưu tiên CompletedAtUtc - StartedAtUtc từ delivery API |
| EndToEndThroughput | ProcessedCount / TotalDurationSeconds |
| DispatchThroughput | ProcessedCount / DispatchDurationSeconds khi duration có |
| BaselineMemoryMb | Median ít nhất 5 mẫu trước request |
| PeakMemoryMb | Max working-set sample trong measurement window |
| AverageMemoryMb | Average các sample trong measurement window |
| MemoryDeltaMb | PeakMemoryMb - BaselineMemoryMb |

Terminal state lấy từ NotificationBatchStatuses hiện tại: COMPLETED,
PARTIAL_FAILED, FAILED. Runner không hard-code bản sao nếu có thể tham chiếu
shared contract; nếu không thể reference Domain do dependency boundary thì gom
mapping tại một file duy nhất và có unit test.

RetryCount không phải metric bắt buộc ở phiên bản đầu vì API không expose tổng
retry của cả success và failed item. Chỉ ghi null/Unavailable kèm lý do; không
suy đoán từ FailedCount.

### 10.2 CSV

| Metric | Cách tính |
| --- | --- |
| ResponseHeadersMs | Request start đến response headers |
| TtfbMs | Request start đến byte body đầu tiên |
| TotalDownloadMs | Request start đến đọc xong/dispose response |
| ResponseBytes | Tổng byte đọc incremental |
| RowsReceived | Số newline dữ liệu sau header, xử lý CSV line ending thống nhất |
| ContentSha256 | Hash incremental của response bytes |
| Baseline/Peak/Average/Delta | Tương tự Batch nhưng monitor course-service |

TTFB dùng HttpCompletionOption.ResponseHeadersRead và đọc stream bằng buffer cố
định. Không dùng GetStringAsync/GetByteArrayAsync trong measurement path.

## 11. Container monitoring

- Resolve container ID bằng docker compose ps -q <service>, không hard-code tên
  dạng ducnm3-...-1.
- Service chính: notification-worker cho Batch, course-service cho CSV.
- Lấy memory usage bằng docker stats --no-stream --format với container ID.
- Parse invariant culture và hỗ trợ KiB/MiB/GiB.
- Default sample interval 1 giây; interval dưới 250ms bị reject.
- Baseline dùng median của ít nhất 5 mẫu trước request.
- Sampler dùng CancellationToken, dispose process, kill process tree khi timeout
  và luôn flush samples trong finally.
- Failure của memory monitor làm run ERROR thay vì xuất memory giả; HTTP result
  có thể vẫn được lưu để điều tra.
- Raw sample gồm timestamp UTC, elapsed ms, service, container ID rút gọn,
  memory bytes và optional CPU percent nếu có sẵn; CPU không phải AC bắt buộc.

## 12. Result layout

~~~text
performance/results/<UTC timestamp>_<git short sha>/
├── metadata.json
├── preparation.log
├── batch/
│   ├── batch-raw.csv
│   ├── batch-summary.csv
│   └── memory-samples.csv
└── csv/
    ├── csv-raw.csv
    ├── csv-summary.csv
    └── memory-samples.csv
~~~

metadata.json tối thiểu gồm:

- timestamp UTC, branch, commit, dirty flag;
- OS, architecture, .NET SDK, Docker/Compose version;
- scenario options, warm-up/runs, timeout và sampling interval;
- exact seed options/random seed/dataset count;
- batch size, chunk concurrency, send concurrency, CSV chunk size;
- Compose project name, service/container/image identifiers;
- endpoint paths và approach order;
- limitation flags như snapshot timing derived from polling.

Result directory được thêm vào .gitignore; chỉ commit curated evidence khi user
yêu cầu rõ.

## 13. Error, timeout và cancellation

- Environment validation fail trước khi reset hoặc benchmark.
- HTTP non-success lưu status/error stage, không parse như success.
- Timeout ghi TIMEOUT, stop sampler và flush partial result.
- Ctrl+C trả exit code 130, stop sampler/process tree và flush partial files.
- Reset/seed failure ghi PREPARATION_ERROR và không bắt đầu stopwatch benchmark.
- CSV correctness mismatch ghi CORRECTNESS_FAILED và loại khỏi summary.
- Metadata phụ như Git SHA/Docker version không lấy được chỉ tạo warning.
- Không log request body, connection string, password hoặc environment secret.

## 14. Kế hoạch triển khai

Mỗi slice chỉ commit sau khi test của slice pass. Commit message dưới đây là đề
xuất và phải giữ tiếng Việt theo rules/git.md.

### Step 1 — Khóa contract và skeleton PerformanceRunner

**Files**

- Create backend/Tools/Lms.PerformanceRunner/Lms.PerformanceRunner.csproj.
- Create backend/Tools/Lms.PerformanceRunner/Program.cs.
- Create backend/Tools/Lms.PerformanceRunner/Configuration/PerformanceOptions.cs.
- Create backend/Tools/Lms.PerformanceRunner/Cli/PerformanceArguments.cs.
- Create backend/Tools/Lms.PerformanceRunner.UnitTests/.
- Modify backend/Lms.sln.

**Red**

- Test reject command không phải batch/csv, users/records ngoài boundary,
  warmup/runs âm, sampling dưới 250ms và --prepare-data thiếu --confirm-reset.

**Green**

- Parser gọn, không thêm CLI parser package.
- Reuse Spectre.Console 0.57.2 giống DataSeeder.
- Main wiring hỗ trợ CancellationToken và exit code ổn định.

**Run**

~~~bash
dotnet test backend/Tools/Lms.PerformanceRunner.UnitTests/Lms.PerformanceRunner.UnitTests.csproj
~~~

**Expected:** parser/unit tests pass; --help không cần service chạy.

**Commit:** Triển khai khung Performance Runner và CLI.

### Step 2 — Mở rộng deterministic Course seed tới 300k

**Files**

- Modify backend/Tools/Lms.DataSeeder/SeedOptions.cs.
- Modify backend/Tools/Lms.DataSeeder/Program.cs.
- Modify backend/Tools/Lms.DataSeeder.UnitTests/SeedOptionsTests.cs.
- Modify backend/Tools/Lms.DataSeeder.UnitTests/DeterministicSeedDataTests.cs.
- Modify backend/Tools/Lms.DataSeeder.IntegrationTests/MySqlSeedRunnerTests.cs
  nếu boundary hiện tại cần evidence thật.
- Modify docs/guide/DATA_SEED_GUIDE.md.

**Red**

- Test --courses 300000 được chấp nhận.
- Test 300001 bị reject.
- Test default Course count vẫn là 100000.
- Test plan/count dùng long và không overflow với 300k Course cùng lesson range.

**Green**

- Tách DefaultCourseCount=100000 và MaximumCourseCount=300000.
- Student maximum giữ 100000.
- Help text và docs ghi đúng boundary.
- Không đổi deterministic ID algorithm hoặc behavior resume.

**Run**

~~~bash
dotnet test backend/Tools/Lms.DataSeeder.UnitTests/Lms.DataSeeder.UnitTests.csproj
dotnet test backend/Tools/Lms.DataSeeder.IntegrationTests/Lms.DataSeeder.IntegrationTests.csproj
~~~

**Expected:** boundary và existing seeder tests pass; integration test dùng
dataset nhỏ, không seed 300k trong CI.

**Commit:** Mở rộng deterministic Course seed tới 300 nghìn bản ghi.

### Step 3 — Thêm buffered CSV endpoint chỉ cho Development

**Files**

- Modify backend/BuildingBlocks/BuildingBlocks.Contracts/Api/ApiRoutes.cs.
- Create backend/Services/Course/CourseService.Api/Endpoints/Performance/BufferedCourseExportBenchmarkEndpoint.cs.
- Create backend/Services/Course/CourseService.Application/UseCases/Courses/Export/BufferedCourseExportHandler.cs.
- Modify backend/Services/Course/CourseService.Application/Repositories/ICourseListRepository.cs.
- Modify backend/Services/Course/CourseService.Infrastructure/Persistence/Repositories/EfCourseListRepository.cs.
- Modify backend/Services/Course/CourseService.Application/DependencyInjection.cs
  nếu handler không tự resolve theo pattern hiện tại.
- Modify backend/Services/Course/CourseService.Api/Program.cs.
- Modify backend/Services/Course/CourseService.ComponentTests/Endpoints/ExportCoursesEndpointComponentTests.cs.
- Modify backend/Services/Course/CourseService.UnitTests/Application/Courses/Export/.
- Create docs/api/course-service/endpoints/get-courses-export-buffered-benchmark.md.
- Modify docs/tests/course-service/component.md và docs/tests/course-service/unit.md.

**Red**

- Development environment: route tồn tại, status filter/CSV contract giống
  streaming.
- Production environment: route trả 404.
- Buffered và streaming cùng seeded rows tạo cùng SHA-256.
- Invalid status trả 400 trước khi repository query.

**Green**

- Register endpoint trong block app.Environment.IsDevelopment() duy nhất.
- Reuse ExportCoursesQuery, CourseExportRow và CsvRowWriter.
- Baseline materialize toàn bộ rows và toàn bộ CSV bytes trước khi response.
- Không sửa behavior/path của endpoint streaming.

**Run**

~~~bash
dotnet test backend/Services/Course/CourseService.UnitTests/CourseService.UnitTests.csproj
dotnet test backend/Services/Course/CourseService.ComponentTests/CourseService.ComponentTests.csproj
~~~

**Expected:** Development contract pass; Production route 404; streaming tests
không regression.

**Commit:** Bổ sung baseline CSV buffered chỉ cho Development.

### Step 4 — Environment validation và dataset coordination

**Files**

- Create backend/Tools/Lms.PerformanceRunner/Environment/EnvironmentValidator.cs.
- Create backend/Tools/Lms.PerformanceRunner/Datasets/DatasetCoordinator.cs.
- Create backend/Tools/Lms.PerformanceRunner/Processes/ProcessRunner.cs.
- Create backend/Tools/Lms.PerformanceRunner/Models/PreparationResult.cs.
- Add unit tests cho process exit/cancel/redaction/required reset confirmation.

**Red**

- Missing repo script, Gateway down, service unhealthy, count mismatch và reset
  chưa confirm đều fail với stage/error rõ.
- Secret-like environment values không xuất hiện trong preparation log.

**Green**

- Resolve repository root an toàn.
- Gọi reset/seed scripts hiện có; không inline SQL xóa dữ liệu.
- Validate exact Student/Course count trước measurement.

**Run**

~~~bash
dotnet test backend/Tools/Lms.PerformanceRunner.UnitTests/Lms.PerformanceRunner.UnitTests.csproj
~~~

**Expected:** mutation luôn cần explicit confirmation; failure không bắt đầu
scenario.

**Commit:** Chuẩn hóa kiểm tra môi trường và chuẩn bị dataset benchmark.

### Step 5 — Container stats monitoring

**Files**

- Create backend/Tools/Lms.PerformanceRunner/Monitoring/ContainerStatsMonitor.cs.
- Create backend/Tools/Lms.PerformanceRunner/Monitoring/ContainerStatsSample.cs.
- Create backend/Tools/Lms.PerformanceRunner/Monitoring/MemoryStatistics.cs.
- Add unit tests cho unit parsing, median baseline, peak/average/delta,
  cancellation và process cleanup abstraction.

**Red**

- Parse KiB/MiB/GiB, invalid/empty output, missing container, cancellation.
- Baseline median và peak calculation có test boundary.

**Green**

- Resolve ID bằng docker compose ps -q.
- Sample docker stats theo interval; không hard-code container name.
- Dispose/kill process tree trong finally.

**Run:** PerformanceRunner unit tests.

**Expected:** calculation deterministic; không cần Docker trong unit tests.

**Commit:** Triển khai monitor memory container cho benchmark.

### Step 6 — Batch Notification scenario

**Files**

- Create backend/Tools/Lms.PerformanceRunner/Http/NotificationBenchmarkClient.cs.
- Create backend/Tools/Lms.PerformanceRunner/Scenarios/BatchNotificationScenario.cs.
- Create backend/Tools/Lms.PerformanceRunner/Models/BatchBenchmarkResult.cs.
- Add tests với HttpMessageHandler double cho 202, polling transitions,
  terminal state, timeout và malformed response.

**Red**

- POST latency không bằng total duration.
- Poll PENDING/SNAPSHOTTING/SNAPSHOT_READY/PROCESSING tới terminal.
- Snapshot duration gắn PollInterval uncertainty.
- Timeout/error vẫn trả partial result.

**Green**

- Dùng Gateway/public route và envelope thực tế.
- Poll snapshot-status rồi delivery-status theo business flow hiện tại.
- Dispatch duration dùng API timestamp; retry total là Unavailable.
- Monitor notification-worker từ trước request tới terminal.

**Run:** PerformanceRunner unit tests và manual smoke với dataset nhỏ.

**Expected:** smoke kết thúc terminal, counters nhất quán, memory samples tồn tại.

**Commit:** Triển khai benchmark toàn lifecycle Notification batch.

### Step 7 — CSV buffered/streaming comparison

**Files**

- Create backend/Tools/Lms.PerformanceRunner/Http/CourseBenchmarkClient.cs.
- Create backend/Tools/Lms.PerformanceRunner/Scenarios/CsvExportScenario.cs.
- Create backend/Tools/Lms.PerformanceRunner/Models/CsvBenchmarkResult.cs.
- Add tests cho ResponseHeadersRead, first-byte timing abstraction, incremental
  hash/bytes/rows và mismatch.

**Red**

- Client không dùng buffered convenience API.
- First byte, total bytes, row count và SHA-256 được tính incremental.
- Content mismatch fail correctness.
- Approach ordering được đảo giữa measured runs.

**Green**

- Download vào Stream.Null khi user không yêu cầu giữ payload.
- Cùng status filter cho hai approach.
- Monitor course-service trong measurement window.

**Run:** PerformanceRunner unit tests và manual smoke 100 Course.

**Expected:** hai approach có cùng hash/row count; metrics tách headers/TTFB/total.

**Commit:** Triển khai so sánh CSV buffered và streaming.

### Step 8 — Reporting, terminal và evidence

**Files**

- Create backend/Tools/Lms.PerformanceRunner/Reporting/TerminalReporter.cs.
- Create backend/Tools/Lms.PerformanceRunner/Reporting/RawResultWriter.cs.
- Create backend/Tools/Lms.PerformanceRunner/Reporting/SummaryCalculator.cs.
- Create backend/Tools/Lms.PerformanceRunner/Reporting/MetadataWriter.cs.
- Create backend/Tools/Lms.PerformanceRunner/Models/BenchmarkRun.cs.
- Modify .gitignore cho performance/results/.
- Add serialization/golden-header tests.

**Red**

- Warm-up bị loại khỏi summary.
- CSV escaping đúng với comma/quote/newline.
- Error/timeout rows vẫn được lưu nhưng không đưa vào success average.
- Metadata redaction test.

**Green**

- Spectre live progress với --plain fallback.
- Summary gồm average/min/max/median và standard deviation cho measured success.
- Flush file atomically theo temp file trong result directory.

**Run:** PerformanceRunner unit tests.

**Expected:** raw/summary headers ổn định; cancel/error vẫn tạo evidence đọc được.

**Commit:** Hoàn thiện báo cáo terminal và evidence benchmark.

### Step 9 — Documentation và smoke automation

**Files**

- Create docs/performance/README.md.
- Create docs/performance/batch-notification-benchmark.md.
- Create docs/performance/csv-export-benchmark.md.
- Modify docs/development/performance.md.
- Modify docs/tests/README.md nếu cần link test catalog.
- Optional create scripts/performance/run-smoke.sh nếu chỉ điều phối command,
  không chứa business logic.

**Content**

- Architecture/Mermaid, prerequisites, safety warning reset.
- Exact command cho 3k/10k/100k batch và 10k/100k/300k CSV.
- Metric definitions, result schema, interpretation và limitations.
- Ghi rõ buffered endpoint Development-only và polling-derived snapshot time.
- Không điền số giả.

**Run**

~~~bash
rg -n \"TBD|TODO|đo thật|hard-code số\" docs/performance
docker compose config
~~~

**Expected:** không placeholder; Compose config valid.

**Commit:** Bổ sung tài liệu vận hành Performance Benchmark Tool.

### Step 10 — Verification cuối

**Run**

~~~bash
dotnet build backend/Lms.sln -m:1
dotnet test backend/Tools/Lms.DataSeeder.UnitTests/Lms.DataSeeder.UnitTests.csproj
dotnet test backend/Tools/Lms.DataSeeder.IntegrationTests/Lms.DataSeeder.IntegrationTests.csproj
dotnet test backend/Tools/Lms.PerformanceRunner.UnitTests/Lms.PerformanceRunner.UnitTests.csproj
dotnet test backend/Services/Course/CourseService.UnitTests/CourseService.UnitTests.csproj
dotnet test backend/Services/Course/CourseService.ComponentTests/CourseService.ComponentTests.csproj
docker compose config
git diff --check
~~~

**Expected**

- Build 0 error.
- Tất cả test liên quan pass.
- Production environment component test xác nhận benchmark route 404.
- Không có result/raw payload hoặc secret bị track.

**Manual smoke**

~~~bash
dotnet run --project backend/Tools/Lms.PerformanceRunner -- \
  batch --users 100 --warmup 0 --runs 1 --prepare-data --confirm-reset

dotnet run --project backend/Tools/Lms.PerformanceRunner -- \
  csv --records 100 --approach both --warmup 0 --runs 1 \
  --prepare-data --confirm-reset
~~~

**Expected:** hai command hoàn tất, tạo metadata/raw/summary/memory samples và
CSV two-approach correctness pass.

**Commit:** Xác minh và hoàn thiện Performance Benchmark Tool.

## 15. Requirements traceability

| Requirement | Plan |
| --- | --- |
| NFR-BATCH-01 | AC-BATCH, Steps 4–6, 8–10 |
| NFR-BATCH-02 | Existing flow audit, AC-BATCH-04, Step 6 |
| NFR-CSV-01 | AC-CSV, Steps 2–3, 7–10 |
| NFR-OBS-01 | Steps 5–6, metadata/error/redaction |
| Reproducible dataset | Section 8, Steps 2 và 4 |
| Total execution time | Section 10.1, Step 6 |
| Real container memory | Section 11, Step 5 |
| Buffered vs streaming fairness | Sections 8.3 và 9, Steps 3 và 7 |
| Raw/summary evidence | Section 12, Step 8 |
| No heavy benchmark in CI | Out of scope, Step 10 chỉ smoke nhỏ |
| Development-only baseline | AC-CSV-04, Section 9, Step 3 |

## 16. Risks và biện pháp

| Risk | Mitigation |
| --- | --- |
| Reset nhầm dữ liệu local | Chỉ Development; hai flag confirm; reuse safety script |
| 300k seed mất lâu | Progress hiện có; batch insert; không chạy CI; ghi preparation time riêng |
| Cache/JIT làm lệch CSV | Warm-up cả hai; alternate approach order; cùng dataset/config |
| Polling làm snapshot time không tuyệt đối | Lưu poll interval và uncertainty; không tuyên bố timestamp chính xác |
| docker stats sampling có overhead | Default 1s; lưu interval; không sample dưới 250ms |
| Batch run trước làm DB lớn dần | Reset/seed mỗi scenario instance |
| Buffered baseline gây OOM | Timeout/cancel; bắt đầu 10k; document intentional risk; không Production |
| Output chứa secret | Allowlist metadata fields và unit test redaction |
| 300k Course kéo theo dữ liệu quan hệ lớn | CSV seed dùng Student count tối thiểu và lesson/enrollment minimum |

## 17. Open questions

Không còn open question chặn planning. Các giá trị runtime như timeout tối đa và
Docker resource limit được cấu hình, lưu trong metadata và chốt khi chạy thật
trên máy demo; chúng không được dùng làm SLO.

## 18. Validate plan

- Local review: pass; đủ scope, AC traceability, file-level steps, verification,
  risks và Won't do/Later.
- MCP validate_plan: chưa hoàn tất vì lần gọi validation bị user hủy; cần chạy
  lại tại review gate trước khi approve.
- Gate: không bắt đầu production code trước khi plan được user approve.

## 19. Revision note

- 2026-08-19: thay bản requirement 46 mục bằng execution plan theo ERBUL26-3050.
- Khóa Course seed maximum 300k, Student maximum 100k.
- Khóa buffered endpoint Development-only và Production 404.
- Khóa reset/seed deterministic, metadata fairness, polling limitation và
  container memory measurement.
- Sửa tên file từ performance-batch-scv.md thành performance-batch-csv.md.
