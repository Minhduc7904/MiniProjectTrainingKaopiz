#!/bin/sh
set -eu

service="${1:?Usage: scaffold.sh <course|student|media|notification|scheduler> [--env-file <path>]}"
project_root="$(CDPATH= cd -- "$(dirname -- "$0")/../../.." && pwd)"
backend_root="$project_root/backend"
env_file="$project_root/.env"
if [ "${2:-}" = "--env-file" ] && [ -n "${3:-}" ]; then env_file="$3"; fi
[ -f "$env_file" ] || { echo "Missing env file: $env_file" >&2; exit 1; }
environment_value() { (cd "$project_root" && docker compose --env-file "$env_file" config --environment | awk -F= -v key="$1" '$1 == key { print substr($0, index($0, "=") + 1); exit }'); }

case "$service" in
  course)
    connection_string="$(environment_value COURSE_DB_LOCAL_CONNECTION_STRING)"
    infrastructure_project="Services/Course/CourseService.Infrastructure/CourseService.Infrastructure.csproj"
    startup_project="Services/Course/CourseService.Api/CourseService.Api.csproj"
    context_name="CourseDbContext"
    namespace="CourseService.Infrastructure.Persistence.Scaffolded"
    context_namespace="CourseService.Infrastructure.Persistence"
    table_args="--table courses --table lessons --table enrollments --table lesson_progresses"
    ;;
  student)
    connection_string="$(environment_value STUDENT_DB_LOCAL_CONNECTION_STRING)"
    infrastructure_project="Services/Student/StudentService.Infrastructure/StudentService.Infrastructure.csproj"
    startup_project="Services/Student/StudentService.Api/StudentService.Api.csproj"
    context_name="StudentDbContext"
    namespace="StudentService.Infrastructure.Persistence.Scaffolded"
    context_namespace="StudentService.Infrastructure.Persistence"
    table_args="--table students"
    ;;
  media)
    connection_string="$(environment_value MEDIA_DB_LOCAL_CONNECTION_STRING)"
    infrastructure_project="Services/Media/MediaService.Infrastructure/MediaService.Infrastructure.csproj"
    startup_project="Services/Media/MediaService.Api/MediaService.Api.csproj"
    context_name="MediaDbContext"
    namespace="MediaService.Infrastructure.Persistence.Scaffolded"
    context_namespace="MediaService.Infrastructure.Persistence"
    table_args="--table media_objects --table media_usages --table media_background_jobs"
    ;;
  notification)
    connection_string="$(environment_value NOTIFICATION_DB_LOCAL_CONNECTION_STRING)"
    infrastructure_project="Services/Notification/NotificationService.Infrastructure/NotificationService.Infrastructure.csproj"
    startup_project="Services/Notification/NotificationService.Api/NotificationService.Api.csproj"
    context_name="NotificationDbContext"
    namespace="NotificationService.Infrastructure.Persistence.Scaffolded"
    context_namespace="NotificationService.Infrastructure.Persistence"
    table_args="--table notification_batches --table notification_batch_items --table notifications"
    ;;
  scheduler)
    connection_string="$(environment_value SCHEDULER_DB_LOCAL_CONNECTION_STRING)"
    infrastructure_project="Services/Scheduler/SchedulerService.Infrastructure/SchedulerService.Infrastructure.csproj"
    startup_project="Services/Scheduler/SchedulerService.Api/SchedulerService.Api.csproj"
    context_name="SchedulerDbContext"
    namespace="SchedulerService.Infrastructure.Persistence.Scaffolded"
    context_namespace="SchedulerService.Infrastructure.Persistence"
    table_args="--table background_jobs --table background_job_runs"
    ;;
  *)
    echo "Unknown service: $service" >&2
    exit 1
    ;;
esac
[ -n "$connection_string" ] || { echo "Missing local connection string in $env_file" >&2; exit 1; }

cd "$backend_root"
dotnet tool run dotnet-ef dbcontext scaffold \
  "$connection_string" \
  Pomelo.EntityFrameworkCore.MySql \
  --project "$infrastructure_project" \
  --startup-project "$startup_project" \
  --output-dir Persistence/Scaffolded \
  --context-dir Persistence \
  --context "$context_name" \
  --namespace "$namespace" \
  --context-namespace "$context_namespace" \
  --no-onconfiguring \
  $table_args \
  --no-build \
  --force
