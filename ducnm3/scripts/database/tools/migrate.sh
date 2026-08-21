#!/bin/sh
set -eu

service="${1:?Usage: migrate.sh <course|student|media|notification|scheduler> [--env-file <path>]}"
project_root="$(CDPATH= cd -- "$(dirname -- "$0")/../../.." && pwd)"
backend_root="$project_root/backend"
env_file="$project_root/.env"
if [ "${2:-}" = "--env-file" ] && [ -n "${3:-}" ]; then env_file="$3"; fi
[ -f "$env_file" ] || { echo "Missing env file: $env_file" >&2; exit 1; }
environment_value() { (cd "$project_root" && docker compose --env-file "$env_file" config --environment | awk -F= -v key="$1" '$1 == key { print substr($0, index($0, "=") + 1); exit }'); }

case "$service" in
  course)
    connection_string="$(environment_value COURSE_DB_LOCAL_CONNECTION_STRING)"
    project="Services/Course/CourseService.Api/CourseService.Api.csproj"
    ;;
  student)
    connection_string="$(environment_value STUDENT_DB_LOCAL_CONNECTION_STRING)"
    project="Services/Student/StudentService.Api/StudentService.Api.csproj"
    ;;
  media)
    connection_string="$(environment_value MEDIA_DB_LOCAL_CONNECTION_STRING)"
    project="Services/Media/MediaService.Api/MediaService.Api.csproj"
    ;;
  notification)
    connection_string="$(environment_value NOTIFICATION_DB_LOCAL_CONNECTION_STRING)"
    project="Services/Notification/NotificationService.Api/NotificationService.Api.csproj"
    ;;
  scheduler)
    connection_string="$(environment_value SCHEDULER_DB_LOCAL_CONNECTION_STRING)"
    project="Services/Scheduler/SchedulerService.Api/SchedulerService.Api.csproj"
    ;;
  *)
    echo "Unknown service: $service" >&2
    exit 1
    ;;
esac
[ -n "$connection_string" ] || { echo "Missing local connection string in $env_file" >&2; exit 1; }

cd "$backend_root"
migration_content_root="$backend_root/$(dirname -- "$project")/bin/Debug/net10.0"
ConnectionStrings__Database="$connection_string" \
Migrations__RunOnly=true \
dotnet run --project "$project" -- --contentRoot "$migration_content_root"
