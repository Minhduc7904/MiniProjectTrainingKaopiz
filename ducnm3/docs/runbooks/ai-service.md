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
