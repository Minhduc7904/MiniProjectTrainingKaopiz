import { studentUi } from '@/theme/student'
import { formatPoints } from '@/utils/quizAnswers'

/** Nhãn và kiểu chip trạng thái của quiz theo bài làm của học viên. */
export function quizStatusOf(quiz) {
  if (quiz.inProgressAttemptId) return { label: 'Đang làm dở', className: studentUi.quizChipWarning }
  if (quiz.submittedAttempts > 0) return { label: `Cao nhất ${formatPoints(quiz.bestScore)}/${formatPoints(quiz.bestMaxScore)}`, className: studentUi.quizChipSuccess }
  return { label: 'Chưa làm', className: studentUi.quizChipQuiet }
}
