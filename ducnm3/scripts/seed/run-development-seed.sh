#!/usr/bin/env bash
set -eu

project_root="$(CDPATH= cd -- "$(dirname -- "$0")/../.." && pwd)"
env_file="$project_root/.env"
if [ "${1:-}" = "--env-file" ] && [ -n "${2:-}" ]; then
  env_file="$2"
  shift 2
fi

if [ ! -f "$env_file" ]; then
  echo "Missing $env_file. Copy .env.example to .env and configure local values first." >&2
  exit 1
fi

confirmed=false
dry_run=false
for argument in "$@"; do
  case "$argument" in
    --confirm)
      confirmed=true
      ;;
    --dry-run)
      dry_run=true
      ;;
  esac
done

if [ "$confirmed" != true ] && [ "$dry_run" != true ]; then
  echo "Refusing to seed without --confirm. Use --dry-run for a read-only check." >&2
  exit 1
fi

environment_value() { (cd "$project_root" && docker compose --env-file "$env_file" config --environment | awk -F= -v key="$1" '$1 == key { print substr($0, index($0, "=") + 1); exit }'); }
environment_name="$(environment_value ASPNETCORE_ENVIRONMENT)"
student_db_name="$(environment_value STUDENT_DB_NAME)"
course_db_name="$(environment_value COURSE_DB_NAME)"
mysql_root_password="$(environment_value MYSQL_ROOT_PASSWORD)"

if [ "${environment_name:-}" != "Development" ]; then
  echo "Refusing to seed outside ASPNETCORE_ENVIRONMENT=Development." >&2
  exit 1
fi

if [[ "$student_db_name" != lms_student*_db ]]; then
  echo "Refusing to seed unexpected Student database." >&2
  exit 1
fi

if [[ "$course_db_name" != lms_course*_db ]]; then
  echo "Refusing to seed unexpected Course database." >&2
  exit 1
fi

cd "$project_root"

echo "Starting MySQL and the services that own seed target schemas..."
docker compose --env-file "$env_file" up -d --build mysql-init course-service student-service

attempt=0
schema_ready=0
while [ "$schema_ready" != "4" ]; do
  schema_ready="$(
    docker compose --env-file "$env_file" exec -T mysql \
      mysql --batch --skip-column-names --protocol=tcp --host=localhost \
      --user=root "--password=$mysql_root_password" \
      --execute="
        SELECT
          (SELECT COUNT(*) FROM information_schema.tables
            WHERE table_schema = '$student_db_name' AND table_name = 'students') +
          (SELECT COUNT(*) FROM information_schema.tables
            WHERE table_schema = '$course_db_name'
              AND table_name IN ('courses', 'lessons', 'enrollments'));
      " 2>/dev/null || true
  )"

  attempt=$((attempt + 1))
  if [ "$attempt" -ge 60 ]; then
    echo "Student and Course schemas did not become ready in time." >&2
    exit 1
  fi

  if [ "$schema_ready" != "4" ]; then
    sleep 2
  fi
done

echo "Building the opt-in development data seeder..."
docker compose --env-file "$env_file" --profile seed build data-seeder

echo "Running deterministic development data seed..."
docker compose --env-file "$env_file" --profile seed run --rm data-seeder \
  Lms.DataSeeder.dll "$@"
