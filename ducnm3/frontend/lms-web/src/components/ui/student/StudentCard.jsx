import { studentUi } from '@/theme/student'

export function StudentCard({ children, className = '' }) {
  return <section className={`${studentUi.card} ${className}`}>{children}</section>
}
