#!/bin/sh
set -eu

service="${1:?Usage: migrate.sh <course|student|media|notification|scheduler>}"
project_root="$(CDPATH= cd -- "$(dirname -- "$0")/../../.." && pwd)"
backend_root="$project_root/backend"

case "$service" in
  course)
    connection_string="${COURSE_DB_LOCAL_CONNECTION_STRING:?COURSE_DB_LOCAL_CONNECTION_STRING must be set}"
    project="Services/Course/CourseService.Api/CourseService.Api.csproj"
    ;;
  student)
    connection_string="${STUDENT_DB_LOCAL_CONNECTION_STRING:?STUDENT_DB_LOCAL_CONNECTION_STRING must be set}"
    project="Services/Student/StudentService.Api/StudentService.Api.csproj"
    ;;
  media)
    connection_string="${MEDIA_DB_LOCAL_CONNECTION_STRING:?MEDIA_DB_LOCAL_CONNECTION_STRING must be set}"
    project="Services/Media/MediaService.Api/MediaService.Api.csproj"
    ;;
  notification)
    connection_string="${NOTIFICATION_DB_LOCAL_CONNECTION_STRING:?NOTIFICATION_DB_LOCAL_CONNECTION_STRING must be set}"
    project="Services/Notification/NotificationService.Api/NotificationService.Api.csproj"
    ;;
  scheduler)
    connection_string="${SCHEDULER_DB_LOCAL_CONNECTION_STRING:?SCHEDULER_DB_LOCAL_CONNECTION_STRING must be set}"
    project="Services/Scheduler/SchedulerService.Api/SchedulerService.Api.csproj"
    ;;
  *)
    echo "Unknown service: $service" >&2
    exit 1
    ;;
esac

cd "$backend_root"
migration_content_root="$backend_root/$(dirname -- "$project")/bin/Debug/net10.0"
ConnectionStrings__Database="$connection_string" \
Migrations__RunOnly=true \
dotnet run --project "$project" -- --contentRoot "$migration_content_root"
