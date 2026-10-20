import { CheckCircle2, CloudOff, LoaderCircle, PencilLine } from 'lucide-react'
import { QUIZ_SAVE_STATE } from '@/constants/quiz'
import { studentUi } from '@/theme/student'
import { formatRelativeTime } from '@/utils/quizAnswers'

/** Chip trạng thái tự lưu; `aria-live` để trình đọc màn hình biết khi lưu xong hoặc lỗi. */
export function QuizSaveStatus({ saveState, lastSavedAt, onRetry }) {
  if (saveState === QUIZ_SAVE_STATE.error) {
    return (
      <span role="status" className={studentUi.quizChipDanger}>
        <CloudOff size={14} aria-hidden="true" />Chưa lưu được
        <button type="button" onClick={onRetry} className="ml-1 cursor-pointer underline outline-none focus-visible:ring-4 focus-visible:ring-student-danger/20">Thử lại</button>
      </span>
    )
  }
  if (saveState === QUIZ_SAVE_STATE.saving) {
    return <span role="status" className={studentUi.quizChipQuiet}><LoaderCircle size={14} aria-hidden="true" className="animate-spin motion-reduce:animate-none" />Đang lưu…</span>
  }
  if (saveState === QUIZ_SAVE_STATE.dirty) {
    return <span role="status" className={studentUi.quizChipQuiet}><PencilLine size={14} aria-hidden="true" />Đang chỉnh sửa</span>
  }
  return (
    <span role="status" className={studentUi.quizChipSuccess}>
      <CheckCircle2 size={14} aria-hidden="true" />{lastSavedAt ? `Đã lưu · ${formatRelativeTime(lastSavedAt)}` : 'Tự động lưu đang bật'}
    </span>
  )
}
