import { Link } from 'react-router-dom'
import { LogOut } from 'lucide-react'
import { StudentEmptyState, StudentShell } from '@/components/ui/student'
import { APP_ROUTES } from '@/constants/appRoutes'
import { studentUi } from '@/theme/student'

export function StudentDashboard() {
  return (
    <StudentShell action={<Link aria-label="Đăng xuất" className={`student-press inline-flex min-h-11 items-center gap-2 rounded-2xl px-3 text-sm ${studentUi.link}`} to={APP_ROUTES.studentLogout}><LogOut aria-hidden="true" size={17} />Đăng xuất</Link>}>
      <section className="grid gap-6 py-4 sm:py-10">
        <div className="student-enter grid max-w-2xl gap-3">
          <p className={studentUi.eyebrow}>Không gian học tập của bạn</p>
          <h1 className={studentUi.title}>Tiếp tục hành trình học</h1>
          <p className={`${studentUi.body} max-w-xl`}>Mỗi bài học hoàn thành sẽ đưa bạn tiến gần hơn tới mục tiêu của mình.</p>
        </div>
        <StudentEmptyState />
      </section>
    </StudentShell>
  )
}
