import { Search } from 'lucide-react'
import { Button } from '@/components/ui/admin/Button'
import { FieldLabel, TextInput } from '@/components/ui/admin/Field'
import { Icon } from '@/components/ui/admin/Icon'
import { QUERY_PARAMS } from '@/constants/queryParams'
import { STUDENT_COPY } from '@/constants/studentCopy'

export function StudentsSearchForm({ query, loading, onChange, onSubmit }) {
  return (
    <form className="flex flex-col gap-2" onSubmit={(event) => { event.preventDefault(); onSubmit(query) }}>
      <FieldLabel htmlFor="student-search" hint="Tìm theo tên hiển thị hoặc email.">{STUDENT_COPY.search}</FieldLabel>
      <div className="flex flex-col gap-2 sm:flex-row">
        <TextInput
          id="student-search"
          name={QUERY_PARAMS.search}
          value={query.search ?? ''}
          disabled={loading}
          placeholder="Ví dụ: student@example.com"
          onChange={(event) => onChange({ ...query, search: event.target.value || undefined, page: 1 })}
        />
        <Button type="submit" disabled={loading} aria-label="Tìm kiếm học viên"><Icon icon={Search} />Tìm</Button>
      </div>
    </form>
  )
}
