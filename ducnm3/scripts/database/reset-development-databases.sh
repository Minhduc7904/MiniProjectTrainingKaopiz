#!/bin/sh
set -eu

if [ "${1:-}" != "--confirm" ] || [ "$#" -ne 1 ]; then
  echo "Refusing to reset databases. Usage: reset-development-databases.sh --confirm" >&2
  exit 1
fi

project_root="$(CDPATH= cd -- "$(dirname -- "$0")/../.." && pwd)"
env_file="$project_root/.env"

if [ ! -f "$env_file" ]; then
  echo "Missing $env_file. Copy .env.example to .env and configure local values first." >&2
  exit 1
fi

set -a
# shellcheck disable=SC1090
. "$env_file"
set +a

if [ "${ASPNETCORE_ENVIRONMENT:-}" != "Development" ]; then
  echo "Refusing to reset databases outside ASPNETCORE_ENVIRONMENT=Development." >&2
  exit 1
fi

require_expected_database()
{
  variable_name="$1"
  actual_value="$2"
  expected_value="$3"

  if [ "$actual_value" != "$expected_value" ]; then
    echo "Refusing to reset: $variable_name must equal $expected_value." >&2
    exit 1
  fi
}

require_expected_database COURSE_DB_NAME "${COURSE_DB_NAME:-}" lms_course_db
require_expected_database STUDENT_DB_NAME "${STUDENT_DB_NAME:-}" lms_student_db
require_expected_database MEDIA_DB_NAME "${MEDIA_DB_NAME:-}" lms_media_db
require_expected_database NOTIFICATION_DB_NAME "${NOTIFICATION_DB_NAME:-}" lms_notification_db
require_expected_database SCHEDULER_DB_NAME "${SCHEDULER_DB_NAME:-}" lms_scheduler_db

cd "$project_root"

docker compose stop \
  api-gateway \
  course-service \
  student-service \
  media-service \
  notification-service \
  scheduler-service

docker compose up -d mysql

attempt=0
until docker compose exec -T mysql \
  mysqladmin ping --silent --host=localhost --user=root "--password=$MYSQL_ROOT_PASSWORD"
do
  attempt=$((attempt + 1))
  if [ "$attempt" -ge 60 ]; then
    echo "MySQL did not become healthy in time." >&2
    exit 1
  fi
  sleep 2
done

docker compose exec -T mysql \
  mysql --protocol=tcp --host=localhost --user=root "--password=$MYSQL_ROOT_PASSWORD" <<SQL
DROP DATABASE IF EXISTS \`${COURSE_DB_NAME}\`;
DROP DATABASE IF EXISTS \`${STUDENT_DB_NAME}\`;
DROP DATABASE IF EXISTS \`${MEDIA_DB_NAME}\`;
DROP DATABASE IF EXISTS \`${NOTIFICATION_DB_NAME}\`;
DROP DATABASE IF EXISTS \`${SCHEDULER_DB_NAME}\`;
SQL

docker compose rm -f mysql-init
docker compose up --force-recreate mysql-init

echo "Development databases were recreated. MinIO data was not changed."
