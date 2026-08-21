#!/usr/bin/env bash
# Mục đích: Wrapper rõ ràng để reset đúng năm database seed.
set -euo pipefail
project_root="$(CDPATH= cd -- "$(dirname -- "$0")/../.." && pwd)"
exec "$project_root/scripts/database/reset-databases.sh" --env-file "$project_root/.env.seed" "$@"
