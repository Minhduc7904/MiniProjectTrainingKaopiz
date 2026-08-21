#!/usr/bin/env bash
# Mục đích: Lưu snapshot nhất quán của năm database runtime để khôi phục vào database seed.
set -euo pipefail
project_root="$(CDPATH= cd -- "$(dirname -- "$0")/../.." && pwd)"
snapshot_dir="$project_root/var/db-seed-snapshots/current"
runtime_env="$project_root/.env"
[[ "${1:-}" == "--confirm" ]] || { echo "Usage: $0 --confirm" >&2; exit 2; }
[[ -f "$runtime_env" ]] || { echo "Missing .env" >&2; exit 2; }
mkdir -p "$snapshot_dir"
cd "$project_root"
docker compose --env-file "$runtime_env" up -d mysql >/dev/null
databases=(lms_course_db lms_student_db lms_media_db lms_notification_db lms_scheduler_db)
for index in "${!databases[@]}"; do
  database="${databases[$index]}"
  started_at="$(date +%s)"
  printf '[%d/5] Backing up %s...\n' "$((index + 1))" "$database"
  docker compose --env-file "$runtime_env" exec -T mysql sh -c "exec mysqldump -uroot -p\"\$MYSQL_ROOT_PASSWORD\" --single-transaction --routines --events --triggers --set-gtid-purged=OFF --databases '$database'" > "$snapshot_dir/$database.sql"
  printf '      Done: %s in %ss\n' "$(du -h "$snapshot_dir/$database.sql" | cut -f1)" "$(( $(date +%s) - started_at ))"
done
sha256sum "$snapshot_dir"/*.sql > "$snapshot_dir/SHA256SUMS"
date -u +%FT%TZ > "$snapshot_dir/CREATED_AT"
echo "Runtime snapshot saved in $snapshot_dir."
