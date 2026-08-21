import { BookOpen } from 'lucide-react'
import { studentUi } from '@/theme/student'
import { StudentCard } from './StudentCard'

export function StudentEmptyState() {
  return (
    <StudentCard className="student-enter grid max-w-2xl gap-4 p-6 sm:p-8">
      <span aria-hidden="true" className={studentUi.emptyIcon}><BookOpen size={24} /></span>
      <div className="grid gap-1">
        <h2 className={studentUi.emptyTitle}>Chờ một khóa học mới</h2>
        <p className={studentUi.body}>Bạn chưa có khóa học đang học.</p>
        <p className={studentUi.caption}>Khi được ghi danh, bài học tiếp theo và tiến độ của bạn sẽ xuất hiện tại đây.</p>
      </div>
    </StudentCard>
  )
}
