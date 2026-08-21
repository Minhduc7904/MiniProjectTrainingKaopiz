#!/usr/bin/env bash
# Mục đích: Khôi phục năm database seed từ snapshot runtime đã được xác nhận.
set -euo pipefail
project_root="$(CDPATH= cd -- "$(dirname -- "$0")/../.." && pwd)"
snapshot_dir="$project_root/var/db-seed-snapshots/current"
seed_env="$project_root/.env.seed"
[[ "${1:-}" == "--confirm" ]] || { echo "Usage: $0 --confirm" >&2; exit 2; }
[[ -f "$seed_env" && -f "$snapshot_dir/SHA256SUMS" ]] || { echo "Missing .env.seed or runtime snapshot." >&2; exit 2; }
cd "$project_root"
(cd "$snapshot_dir" && sha256sum --check SHA256SUMS)
docker compose --env-file "$seed_env" up -d mysql >/dev/null
databases=(lms_course lms_student lms_media lms_notification lms_scheduler)
for index in "${!databases[@]}"; do
  database="${databases[$index]}"
  source_name="${database}_db"
  target_name="${database}_seed_db"
  started_at="$(date +%s)"
  printf '[%d/5] Restoring %s -> %s...\n' "$((index + 1))" "$source_name" "$target_name"
  docker compose --env-file "$seed_env" exec -T mysql sh -c "mysql -uroot -p\"\$MYSQL_ROOT_PASSWORD\" -e 'DROP DATABASE IF EXISTS \`$target_name\`; CREATE DATABASE \`$target_name\`;'"
  sed "s/\`$source_name\`/\`$target_name\`/g" "$snapshot_dir/$source_name.sql" | docker compose --env-file "$seed_env" exec -T mysql sh -c "mysql -uroot -p\"\$MYSQL_ROOT_PASSWORD\""
  printf '      Done in %ss\n' "$(( $(date +%s) - started_at ))"
done
docker compose --env-file "$seed_env" rm -sf mysql-init >/dev/null
docker compose --env-file "$seed_env" up --force-recreate mysql-init >/dev/null
echo "Seed databases restored. Start test stack with: docker compose --env-file .env.seed up -d --build"
