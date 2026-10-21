# Runbook — AI Service

## Biến môi trường

| Biến | Ý nghĩa |
| --- | --- |
| `AI_DB_NAME`, `AI_DB_USER`, `AI_DB_PASSWORD` | Database/user riêng `lms_ai_db` do `mysql-init` tạo. |
| `AI_DB_CONNECTION_STRING` | Connection string trong Docker; `AI_DB_LOCAL_CONNECTION_STRING` dùng khi chạy local. |
| `AI_ENCRYPTION_KEY` | Master key AES-256-GCM, base64 32 byte. Sinh bằng `openssl rand -base64 32`. |

## Master key `AI_ENCRYPTION_KEY`

- Bắt buộc; AI Service từ chối khởi động nếu thiếu hoặc không phải base64 32 byte.
- **Mất hoặc đổi key thì không giải mã được API key đã lưu.** Khi đó phải nhập lại API key cho từng provider (PUT provider kèm `apiKey`). Hãy sao lưu key ở nơi an toàn, tách khỏi backup database.
- Không commit `.env`; `.env.example` chỉ có placeholder.
- Đổi key chủ động (rotation) chưa có công cụ; hiện cách duy nhất là đổi key rồi nhập lại key của mọi provider.

## Chạy và kiểm tra

```bash
docker compose up -d --build ai-service api-gateway
curl http://localhost:5107/health
curl -H "X-Actor-Type: ADMIN" -H "X-Actor-Id: <uuid>" http://localhost:5100/ai/api/ai/options
```

Swagger của service nằm trong Swagger tổng của Gateway (`ai-service`).

## Seed/reset

`lms_ai_db` không nằm trong `reset-databases.sh`, `restore-seed-from-backup.sh`, `backup-runtime-to-seed.sh`: đó là cấu hình mã hóa, không phải dữ liệu mẫu. Reset thủ công bằng cách drop/tạo lại database rồi để migration chạy lại, và nhập lại API key.

## Kiểm tra kết nối provider

`POST /ai/api/ai/providers/health-checks` (tất cả) và `POST /ai/api/ai/providers/{providerId}/health-check` (một provider) gọi thử `GET {baseUrl}/models` bằng API key đã lưu, timeout 6 giây. Trang **AI provider** và **Dashboard** gọi endpoint đầu tiên mỗi lần mở trang. Kết quả `AUTH_FAILED` nghĩa là key bị từ chối (nhập lại key), `UNREACHABLE` nghĩa là container `ai-service` không ra được tới base URL (kiểm tra DNS/mạng/proxy); kết quả không được lưu vào database.

## Gọi model (`POST /ai/api/ai/completions`)

- Mỗi tác vụ AI có khóa binding riêng: `document-extraction`, `lesson-synthesis`, `quiz-authoring`. Gán model ở trang **AI → Gán model**. Thiếu binding thì mọi lần gọi trả `409 AI_TASK_NOT_CONFIGURED`.
- `document-extraction` cần model **đọc được PDF/ảnh**; `lesson-synthesis` và `quiz-authoring` cần model **hỗ trợ tool (function calling)**. Model nhỏ gọi tool kém sẽ làm job `FAILED` (`AI_NO_RESULT` hoặc `AI_TOO_MANY_STEPS`).
- Provider được gọi qua chuẩn OpenAI-compatible `chat/completions`; ảnh gửi bằng `image_url`, PDF gửi bằng phần `file`. **Chưa phải provider nào cũng nhận PDF trực tiếp**: nếu bị từ chối sẽ thấy `502 AI_PROVIDER_REJECTED_REQUEST` ở tài liệu `FAILED`; khi đó dùng model/provider khác cho `document-extraction` hoặc chuyển PDF sang ảnh/DOCX trước khi tải lên.
- Timeout 240 giây mỗi lần gọi, không retry tự động (tránh tính phí hai lần).
- Mỗi lần gọi ghi `ai_invocations` (task, model, token, độ trễ, mã lỗi) — dùng để theo dõi chi phí:

```sql
SELECT task_name, model_name, status, COUNT(*) AS calls, SUM(prompt_tokens) AS prompt_tokens,
       SUM(completion_tokens) AS completion_tokens, ROUND(AVG(latency_ms)) AS avg_latency_ms
FROM ai_invocations GROUP BY task_name, model_name, status;
```

Quy trình dùng tính năng ở phía Course: [`lesson-ai-authoring.md`](lesson-ai-authoring.md).

