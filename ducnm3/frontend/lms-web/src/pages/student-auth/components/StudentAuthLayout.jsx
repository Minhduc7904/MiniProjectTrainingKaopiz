import { StudentCard, StudentShell } from '@/components/ui/student'
import { studentUi } from '@/theme/student'

export function StudentAuthLayout({ title, children }) {
  return (
    <StudentShell compact>
      <StudentCard className="student-enter w-full p-6 sm:p-8">
        <div className="grid gap-2">
          <p className={studentUi.eyebrow}>LMS dành cho học viên</p>
          <h1 className={studentUi.title}>{title}</h1>
        </div>
        {children}
      </StudentCard>
    </StudentShell>
  )
}
