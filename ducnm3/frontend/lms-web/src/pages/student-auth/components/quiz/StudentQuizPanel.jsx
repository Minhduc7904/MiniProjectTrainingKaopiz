import { useState } from 'react'
import { StudentLoadingState } from '@/components/ui/student'
import { useQuizAttemptDetail, useQuizAttemptHistory, useStudentQuizDetail } from '@/hooks/quizzes/useStudentQuizData'
import { ATTEMPT_STATUS } from '@/constants/quiz'
import { studentUi } from '@/theme/student'
import { QuizOverview } from './QuizOverview'
import { QuizResultView } from './QuizResultView'
import { QuizTakingView } from './QuizTakingView'

const OVERVIEW = { mode: 'overview' }

function ReviewAttempt({ courseId, lessonId, quiz, attemptId, onRetake, onHistory, onNextLesson }) {
  const { detail, loading, error, reload } = useQuizAttemptDetail(courseId, lessonId, quiz.id, attemptId)
  if (loading || (!detail && !error)) return <StudentLoadingState title="Đang tải bài làm..." />
  if (error) return <div className={studentUi.quizShell}><p role="alert" className={studentUi.error}>{error.message}</p><button type="button" onClick={reload} className={`mt-4 ${studentUi.quizGhostButton}`}>Thử lại</button></div>
  return <QuizResultView quiz={quiz} result={detail.result} onRetake={onRetake} onHistory={onHistory} onNextLesson={onNextLesson} />
}

/**
 * Trạng thái quiz trong trang học bài: tổng quan → làm bài → kết quả, hoặc xem lại một lần làm cũ.
 * Không rời trang học bài nên thanh bài học bên trái vẫn dùng được.
 */
export function StudentQuizPanel({ courseId, lessonId, quizId, onChanged, onNextLesson }) {
  const { quiz, loading, error, reload } = useStudentQuizDetail(courseId, lessonId, quizId)
  const history = useQuizAttemptHistory(courseId, lessonId, quizId)
  const [view, setView] = useState(OVERVIEW)

  if (error) return <div className={studentUi.quizShell}><p role="alert" className={studentUi.error}>{error.message}</p><button type="button" onClick={reload} className={`mt-4 ${studentUi.quizGhostButton}`}>Thử lại</button></div>
  if (loading || !quiz) return <StudentLoadingState title="Đang tải bài trắc nghiệm..." />

  const backToOverview = () => { history.reload(); onChanged?.(); setView(OVERVIEW) }
  const startTaking = () => setView({ mode: 'taking', nonce: Date.now() })

  if (view.mode === 'taking') {
    return (
      <QuizTakingView
        key={view.nonce}
        courseId={courseId}
        lessonId={lessonId}
        quiz={quiz}
        onExit={backToOverview}
        onSubmitted={(data) => { history.reload(); onChanged?.(); setView({ mode: 'result', result: data.result }) }}
      />
    )
  }
  if (view.mode === 'result') {
    return <QuizResultView quiz={quiz} result={view.result} onRetake={startTaking} onHistory={backToOverview} onNextLesson={onNextLesson} />
  }
  if (view.mode === 'review') {
    return <ReviewAttempt courseId={courseId} lessonId={lessonId} quiz={quiz} attemptId={view.attemptId} onRetake={startTaking} onHistory={backToOverview} onNextLesson={onNextLesson} />
  }

  return (
    <QuizOverview
      quiz={quiz}
      attempts={history.attempts}
      historyLoading={history.loading}
      onStart={startTaking}
      onOpenAttempt={(attempt) => (attempt.status === ATTEMPT_STATUS.inProgress ? startTaking() : setView({ mode: 'review', attemptId: attempt.id }))}
    />
  )
}
