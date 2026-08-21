#!/usr/bin/env bash
# Mục đích: Tạo .env.seed từ .env bằng cách chỉ đổi target database sang snapshot seed.
set -euo pipefail
project_root="$(CDPATH= cd -- "$(dirname -- "$0")/../.." && pwd)"
source_file="$project_root/.env"
target_file="$project_root/.env.seed"
if [[ ! -f "$source_file" || -e "$target_file" ]]; then
  echo "Require .env and refuse to overwrite existing .env.seed." >&2; exit 1
fi
sed \
  -e 's/^\(COURSE_DB_NAME=\).*/\1lms_course_seed_db/' \
  -e 's/^\(STUDENT_DB_NAME=\).*/\1lms_student_seed_db/' \
  -e 's/^\(MEDIA_DB_NAME=\).*/\1lms_media_seed_db/' \
  -e 's/^\(NOTIFICATION_DB_NAME=\).*/\1lms_notification_seed_db/' \
  -e 's/^\(SCHEDULER_DB_NAME=\).*/\1lms_scheduler_seed_db/' \
  -e 's/Database=lms_course_db/Database=lms_course_seed_db/g' \
  -e 's/Database=lms_student_db/Database=lms_student_seed_db/g' \
  -e 's/Database=lms_media_db/Database=lms_media_seed_db/g' \
  -e 's/Database=lms_notification_db/Database=lms_notification_seed_db/g' \
  -e 's/Database=lms_scheduler_db/Database=lms_scheduler_seed_db/g' \
  "$source_file" > "$target_file"
echo "Created $target_file."
