import { LoaderCircle } from 'lucide-react'
import { studentUi } from '@/theme/student'

export function StudentLoadingState({ title = 'Đang chuẩn bị hành trình học' }) {
  return <div aria-live="polite" className="student-enter grid justify-items-center gap-3 py-12 text-center"><LoaderCircle aria-hidden="true" className={`animate-spin ${studentUi.loadingIcon}`} size={28} /><p className={studentUi.body}>{title}</p></div>
}
