#!/bin/sh
set -eu

service="${1:?Usage: scaffold.sh <course|student|media|notification>}"
project_root="$(CDPATH= cd -- "$(dirname -- "$0")/../../.." && pwd)"
backend_root="$project_root/backend"

case "$service" in
  course)
    connection_string="${COURSE_DB_LOCAL_CONNECTION_STRING:?COURSE_DB_LOCAL_CONNECTION_STRING must be set}"
    infrastructure_project="Services/Course/CourseService.Infrastructure/CourseService.Infrastructure.csproj"
    startup_project="Services/Course/CourseService.Api/CourseService.Api.csproj"
    context_name="CourseDbContext"
    namespace="CourseService.Infrastructure.Persistence.Scaffolded"
    context_namespace="CourseService.Infrastructure.Persistence"
    table_args="--table courses --table lessons --table enrollments --table lesson_progresses"
    ;;
  student)
    connection_string="${STUDENT_DB_LOCAL_CONNECTION_STRING:?STUDENT_DB_LOCAL_CONNECTION_STRING must be set}"
    infrastructure_project="Services/Student/StudentService.Infrastructure/StudentService.Infrastructure.csproj"
    startup_project="Services/Student/StudentService.Api/StudentService.Api.csproj"
    context_name="StudentDbContext"
    namespace="StudentService.Infrastructure.Persistence.Scaffolded"
    context_namespace="StudentService.Infrastructure.Persistence"
    table_args="--table students"
    ;;
  media)
    connection_string="${MEDIA_DB_LOCAL_CONNECTION_STRING:?MEDIA_DB_LOCAL_CONNECTION_STRING must be set}"
    infrastructure_project="Services/Media/MediaService.Infrastructure/MediaService.Infrastructure.csproj"
    startup_project="Services/Media/MediaService.Api/MediaService.Api.csproj"
    context_name="MediaDbContext"
    namespace="MediaService.Infrastructure.Persistence.Scaffolded"
    context_namespace="MediaService.Infrastructure.Persistence"
    table_args="--table media_objects --table media_usages"
    ;;
  notification)
    connection_string="${NOTIFICATION_DB_LOCAL_CONNECTION_STRING:?NOTIFICATION_DB_LOCAL_CONNECTION_STRING must be set}"
    infrastructure_project="Services/Notification/NotificationService.Infrastructure/NotificationService.Infrastructure.csproj"
    startup_project="Services/Notification/NotificationService.Api/NotificationService.Api.csproj"
    context_name="NotificationDbContext"
    namespace="NotificationService.Infrastructure.Persistence.Scaffolded"
    context_namespace="NotificationService.Infrastructure.Persistence"
    table_args="--table notification_jobs --table notification_job_items --table notifications"
    ;;
  *)
    echo "Unknown service: $service" >&2
    exit 1
    ;;
esac

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
