import { Search } from 'lucide-react'
import { Button } from '@/components/ui/admin/Button'
import { FieldLabel, TextInput } from '@/components/ui/admin/Field'
import { Icon } from '@/components/ui/admin/Icon'
import { COURSE_COPY } from '@/constants/courseCopy'
import { QUERY_PARAMS } from '@/constants/queryParams'

export function CourseSearchForm({ query, loading, onChange, onSubmit }) {
  return (
    <form
      className="flex flex-col gap-2"
      onSubmit={(event) => {
        event.preventDefault()
        onSubmit(query)
      }}
    >
      <FieldLabel htmlFor="course-search" hint={COURSE_COPY.searchHint}>
        {COURSE_COPY.search}
      </FieldLabel>
      <div className="flex flex-col gap-2 sm:flex-row">
        <TextInput
          id="course-search"
          name={QUERY_PARAMS.search}
          value={query.search ?? ''}
          disabled={loading}
          placeholder="Ví dụ: Backend Fundamentals"
          onChange={(event) =>
            onChange({
              ...query,
              search: event.target.value || undefined,
              page: 1,
            })
          }
        />
        <Button type="submit" disabled={loading} aria-label="Tìm kiếm khóa học">
          <Icon icon={Search} />
          {COURSE_COPY.searchAction}
        </Button>
      </div>
    </form>
  )
}
