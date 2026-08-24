import { Search } from 'lucide-react'
import { StudentInput } from '@/components/ui/student/StudentInput'
import { QUERY_PARAMS } from '@/constants/queryParams'
import { studentUi } from '@/theme/student'

export function StudentCourseCatalogSearch({ query, loading, onChange, onSubmit }) {
  return (
    <form className="student-enter grid max-w-2xl gap-3 sm:grid-cols-[minmax(0,1fr)_auto] sm:items-end" onSubmit={(event) => { event.preventDefault(); onSubmit(query) }}>
      <StudentInput
        id="student-course-search"
        label="Tìm khóa học"
        hint="Tìm theo tên khóa học để khám phá nội dung phù hợp."
        name={QUERY_PARAMS.search}
        value={query.search ?? ''}
        disabled={loading}
        placeholder="Ví dụ: Backend Fundamentals"
        onChange={(event) => onChange({ ...query, search: event.target.value || undefined, page: 1 })}
      />
      <button className={studentUi.searchAction} type="submit" disabled={loading} aria-label="Tìm kiếm khóa học">
        <Search aria-hidden="true" size={18} />
        Tìm
      </button>
    </form>
  )
}
