#!/bin/sh
set -eu

project_root="$(CDPATH= cd -- "$(dirname -- "$0")/../.." && pwd)"
env_file="$project_root/.env"

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

set -a
# shellcheck disable=SC1090
. "$env_file"
set +a

if [ "${ASPNETCORE_ENVIRONMENT:-}" != "Development" ]; then
  echo "Refusing to seed outside ASPNETCORE_ENVIRONMENT=Development." >&2
  exit 1
fi

if [ "${STUDENT_DB_NAME:-}" != "lms_student_db" ]; then
  echo "Refusing to seed: STUDENT_DB_NAME must equal lms_student_db." >&2
  exit 1
fi

if [ "${COURSE_DB_NAME:-}" != "lms_course_db" ]; then
  echo "Refusing to seed: COURSE_DB_NAME must equal lms_course_db." >&2
  exit 1
fi

cd "$project_root"

echo "Starting MySQL and the services that own seed target schemas..."
docker compose up -d --build mysql-init course-service student-service

attempt=0
schema_ready=0
while [ "$schema_ready" != "4" ]; do
  schema_ready="$(
    docker compose exec -T mysql \
      mysql --batch --skip-column-names --protocol=tcp --host=localhost \
      --user=root "--password=$MYSQL_ROOT_PASSWORD" \
      --execute="
        SELECT
          (SELECT COUNT(*) FROM information_schema.tables
            WHERE table_schema = 'lms_student_db' AND table_name = 'students') +
          (SELECT COUNT(*) FROM information_schema.tables
            WHERE table_schema = 'lms_course_db'
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
docker compose --profile seed build data-seeder

echo "Running deterministic development data seed..."
docker compose --profile seed run --rm data-seeder \
  Lms.DataSeeder.dll "$@"
