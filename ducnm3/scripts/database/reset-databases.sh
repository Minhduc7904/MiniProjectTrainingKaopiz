#!/usr/bin/env bash
# Mục đích: Reset an toàn năm database của một profile env local, không tác động MinIO.
set -euo pipefail

project_root="$(CDPATH= cd -- "$(dirname -- "$0")/../.." && pwd)"
env_file="$project_root/.env"
[[ "${1:-}" == "--env-file" ]] && { env_file="$2"; shift 2; }
[[ "${1:-}" == "--confirm" && "$#" == 1 ]] || { echo "Usage: $0 [--env-file .env|.env.seed] --confirm" >&2; exit 2; }
[[ -f "$env_file" ]] || { echo "Missing env file: $env_file" >&2; exit 2; }

value() { docker compose --env-file "$env_file" config --environment | awk -F= -v key="$1" '$1 == key { print substr($0, index($0, "=") + 1); exit }'; }
environment_name="$(value ASPNETCORE_ENVIRONMENT)"
[[ "${environment_name:-Development}" == Development ]] || { echo "Refusing outside Development." >&2; exit 2; }
names=("$(value COURSE_DB_NAME)" "$(value STUDENT_DB_NAME)" "$(value MEDIA_DB_NAME)" "$(value NOTIFICATION_DB_NAME)" "$(value SCHEDULER_DB_NAME)")
for name in "${names[@]}"; do [[ "$name" =~ ^lms_(course|student|media|notification|scheduler)(_seed)?_db$ ]] || { echo "Refusing unexpected database: $name" >&2; exit 2; }; done

cd "$project_root"
echo "Reset profile: $env_file"
printf 'Target databases: %s\n' "${names[*]}"
docker compose --env-file "$env_file" stop api-gateway course-service student-service media-service media-worker notification-service notification-worker scheduler-service scheduler-worker admin-service >/dev/null || true
docker compose --env-file "$env_file" up -d mysql >/dev/null
for index in "${!names[@]}"; do
  printf '[%d/5] Dropping %s...\n' "$((index + 1))" "${names[$index]}"
  docker compose --env-file "$env_file" exec -T mysql sh -c "mysql -uroot -p\"\$MYSQL_ROOT_PASSWORD\" -e \"DROP DATABASE IF EXISTS ${names[$index]}; CREATE DATABASE ${names[$index]};\""
done
docker compose --env-file "$env_file" rm -sf mysql-init >/dev/null
docker compose --env-file "$env_file" up --force-recreate mysql-init >/dev/null
echo 'Database reset completed. Start the selected profile to apply migrations.'
