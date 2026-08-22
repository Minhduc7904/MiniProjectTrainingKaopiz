import { GraduationCap } from 'lucide-react'
import { studentUi } from '@/theme/student'

export function StudentShell({ children, action, compact = false, navigation, scrollable = false }) {
  return (
    <main className={`student-theme ${studentUi.page} ${scrollable ? studentUi.scrollPage : ''}`}>
      <a className={studentUi.skipLink} href="#student-content">Chuyển đến nội dung chính</a>
      <div className={`${studentUi.shell} ${compact ? 'max-w-xl justify-center' : ''}`}>
        <header className={`${studentUi.topbar} ${compact ? studentUi.topbarCompact : ''}`}>
          <div className="flex items-center gap-2">
            <span aria-hidden="true" className={studentUi.brandMark}><GraduationCap size={20} strokeWidth={2} /></span>
            <span className={studentUi.brand}>Hành trình học</span>
          </div>
          {navigation}
          {action ? <div className={studentUi.headerAction}>{action}</div> : null}
        </header>
        <div id="student-content" tabIndex={-1}>{children}</div>
      </div>
    </main>
  )
}
