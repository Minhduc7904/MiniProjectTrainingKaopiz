import { ArrowLeft, BadgeCheck, Mail, UserRound } from 'lucide-react'
import { Link } from 'react-router-dom'
import { StudentAccountMenu, StudentCard, StudentNavigation, StudentShell } from '@/components/ui/student'
import { APP_ROUTES } from '@/constants/appRoutes'
import { studentUi } from '@/theme/student'

export function StudentProfile({ student }) {
  return (
    <StudentShell navigation={<StudentNavigation />} action={<StudentAccountMenu displayName={student.displayName} email={student.email} logoutTo={APP_ROUTES.studentLogout} profileTo={APP_ROUTES.studentProfile} />}>
      <section className="grid max-w-2xl gap-6 py-4 sm:py-10">
        <Link className={`inline-flex min-h-11 w-fit items-center gap-2 ${studentUi.link}`} to={APP_ROUTES.studentHome}>
          <ArrowLeft aria-hidden="true" size={17} />
          Về trang học
        </Link>
        <StudentCard className="student-enter grid gap-6 p-6 sm:p-8">
          <div className="grid gap-2">
            <p className={studentUi.eyebrow}>Hồ sơ học viên</p>
            <h1 className={studentUi.title}>Thông tin của bạn</h1>
            <p className={studentUi.body}>Thông tin này được xác minh khi bạn vào khu vực học.</p>
          </div>
          <dl className="grid gap-4 sm:grid-cols-2">
            <div className="grid gap-1">
              <dt className={`inline-flex items-center gap-2 ${studentUi.profileLabel}`}><UserRound aria-hidden="true" size={16} />Tên hiển thị</dt>
              <dd className={studentUi.profileValue}>{student.displayName}</dd>
            </div>
            <div className="grid gap-1">
              <dt className={`inline-flex items-center gap-2 ${studentUi.profileLabel}`}><Mail aria-hidden="true" size={16} />Email</dt>
              <dd className={studentUi.profileValue}>{student.email}</dd>
            </div>
            <div className="grid gap-1">
              <dt className={`inline-flex items-center gap-2 ${studentUi.profileLabel}`}><BadgeCheck aria-hidden="true" size={16} />Trạng thái</dt>
              <dd className={studentUi.profileValue}>{student.status}</dd>
            </div>
          </dl>
        </StudentCard>
      </section>
    </StudentShell>
  )
}
