import { Dropdown } from '@/components/ui/Dropdown'
import { STUDENT_COPY } from '@/constants/studentCopy'
import { SORT_DIRECTIONS, STUDENT_SORT_BY } from '@/constants/queryParams'
import { STUDENT_STATUS, STUDENT_STATUS_LABELS } from '@/constants/studentStatus'
import { UI_LABELS } from '@/constants/ui'

const STATUS_OPTIONS = [
  { value: '', label: UI_LABELS.all },
  ...Object.values(STUDENT_STATUS).map((status) => ({
    value: status,
    label: STUDENT_STATUS_LABELS[status],
  })),
]

const SORT_BY_OPTIONS = [
  { value: STUDENT_SORT_BY.createdAt, label: STUDENT_COPY.createdAt },
  { value: STUDENT_SORT_BY.displayName, label: STUDENT_COPY.displayName },
  { value: STUDENT_SORT_BY.email, label: STUDENT_COPY.email },
]

const SORT_DIRECTION_OPTIONS = [
  { value: SORT_DIRECTIONS.desc, label: STUDENT_COPY.descending },
  { value: SORT_DIRECTIONS.asc, label: STUDENT_COPY.ascending },
]

export function StudentsFilters({ query, loading, onChange }) {
  return (
    <div className="flex flex-col gap-3">
      <Dropdown
        label={STUDENT_COPY.status}
        className="w-full min-w-0"
        value={query.status ?? ''}
        options={STATUS_OPTIONS}
        disabled={loading}
        onChange={(status) =>
          onChange({
            ...query,
            status: status || undefined,
            page: 1,
          })
        }
      />
      <Dropdown
        label={STUDENT_COPY.sortBy}
        className="w-full min-w-0"
        value={query.sortBy}
        options={SORT_BY_OPTIONS}
        disabled={loading}
        onChange={(sortBy) =>
          onChange({
            ...query,
            sortBy,
            page: 1,
          })
        }
      />
      <Dropdown
        label={STUDENT_COPY.direction}
        className="w-full min-w-0"
        value={query.sortDirection}
        options={SORT_DIRECTION_OPTIONS}
        disabled={loading}
        onChange={(sortDirection) =>
          onChange({
            ...query,
            sortDirection,
            page: 1,
          })
        }
      />
    </div>
  )
}
