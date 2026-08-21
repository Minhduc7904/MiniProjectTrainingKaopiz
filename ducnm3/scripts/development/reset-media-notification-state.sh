#!/usr/bin/env bash
# Mục đích: Reset có chủ đích Media/Notification DB, Media MinIO và RabbitMQ local; giữ nguyên Course DB.
set -euo pipefail

if [[ "${1:-}" != "--confirm" ]]; then
  echo "Usage: $0 --confirm" >&2
  exit 2
fi

project_root="$(CDPATH= cd -- "$(dirname -- "$0")/../.." && pwd)"
cd "$project_root"
environment_value() {
  docker compose config --environment | awk -F= -v key="$1" '$1 == key { print substr($0, index($0, "=") + 1); exit }'
}
media_db_name="$(environment_value MEDIA_DB_NAME)"
notification_db_name="$(environment_value NOTIFICATION_DB_NAME)"
media_db_user="$(environment_value MEDIA_DB_USER)"
notification_db_user="$(environment_value NOTIFICATION_DB_USER)"
environment_name="$(environment_value ASPNETCORE_ENVIRONMENT)"
if [[ "${environment_name:-Development}" != "Development" ]]; then
  echo "Only Development may be reset." >&2
  exit 2
fi

docker compose stop course-service student-service notification-service notification-worker media-service media-worker scheduler-service scheduler-worker api-gateway admin-service
docker compose up -d mysql minio
docker compose exec -T mysql sh -c "mysql -uroot -p\"\$MYSQL_ROOT_PASSWORD\" -e \"DROP DATABASE IF EXISTS \\\`$media_db_name\\\`; DROP DATABASE IF EXISTS \\\`$notification_db_name\\\`; CREATE DATABASE \\\`$media_db_name\\\`; CREATE DATABASE \\\`$notification_db_name\\\`; GRANT ALL PRIVILEGES ON \\\`$media_db_name\\\`.* TO '$media_db_user'@'%'; GRANT ALL PRIVILEGES ON \\\`$notification_db_name\\\`.* TO '$notification_db_user'@'%'; FLUSH PRIVILEGES;\""
docker compose rm -sf rabbitmq
docker volume rm "$(basename "$project_root")_rabbitmq-data"
docker compose up -d rabbitmq mysql-init
docker compose run --rm --entrypoint /bin/sh minio-init -c 'mc alias set local "$MINIO_ENDPOINT" "$MINIO_ROOT_USER" "$MINIO_ROOT_PASSWORD"; for bucket in "$MINIO_IMAGE_BUCKET" "$MINIO_VIDEO_BUCKET" "$MINIO_DOCUMENT_BUCKET" "$MINIO_AUDIO_BUCKET" "$MINIO_OTHER_BUCKET"; do mc rm --recursive --force "local/$bucket" || true; mc rb --force "local/$bucket" || true; done'
docker compose run --rm minio-init
docker compose up -d --build

echo "Reset completed: Media DB, Notification DB, Media buckets and RabbitMQ were recreated. Course DB was preserved."
