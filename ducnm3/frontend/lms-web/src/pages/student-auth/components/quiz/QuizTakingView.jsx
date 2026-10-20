import { useCallback, useMemo, useState } from 'react'
import { ArrowLeft, ArrowRight, Keyboard, Send } from 'lucide-react'
import { StudentLoadingState } from '@/components/ui/student'
import { QUIZ_COPY } from '@/constants/quiz'
import { useQuizAttempt } from '@/hooks/quizzes/useQuizAttempt'
import { useQuizKeyboard } from '@/hooks/quizzes/useQuizKeyboard'
import { studentUi } from '@/theme/student'
import { countAnswered, formatRelativeTime, unansweredIndexes } from '@/utils/quizAnswers'
import { QuestionCard } from './QuestionCard'
import { QuestionNavigator } from './QuestionNavigator'
import { QuizSaveStatus } from './QuizSaveStatus'
import { QuizSubmitDialog } from './QuizSubmitDialog'

/** Màn làm bài tập trung: thanh tiến độ + trạng thái lưu, lưới câu hỏi, thẻ câu hỏi và thanh điều hướng dính đáy. */
export function QuizTakingView({ courseId, lessonId, quiz, onSubmitted, onExit }) {
  const attempt = useQuizAttempt({ courseId, lessonId, quiz })
  const [currentIndex, setCurrentIndex] = useState(0)
  const [flagged, setFlagged] = useState(() => new Set())
  const [confirming, setConfirming] = useState(false)
  const questions = quiz.questions
  const question = questions[currentIndex]
  const answered = countAnswered(questions, attempt.answers)
  const blankIndexes = useMemo(() => unansweredIndexes(questions, attempt.answers), [questions, attempt.answers])
  const ready = attempt.start.status === 'ready'

  const goTo = useCallback((index) => setCurrentIndex(Math.min(Math.max(index, 0), questions.length - 1)), [questions.length])
  const toggleFlag = () => setFlagged((current) => {
    const next = new Set(current)
    if (!next.delete(question.id)) next.add(question.id)
    return next
  })

  useQuizKeyboard({
    enabled: ready && !confirming,
    onPrevious: () => goTo(currentIndex - 1),
    onNext: () => goTo(currentIndex + 1),
    onSelectIndex: (index) => { const option = question.options[index]; if (option) attempt.select(question, option.id) },
    onSubmit: () => setConfirming(true),
  })

  const confirmSubmit = async () => {
    try {
      const data = await attempt.submitAttempt()
      if (data) { setConfirming(false); onSubmitted(data) }
    } catch {
      // Lỗi đã hiển thị trong hộp thoại; đáp án vẫn được giữ để thử nộp lại.
    }
  }

  if (attempt.start.status === 'error') {
    return (
      <div className={studentUi.quizShell}>
        <p role="alert" className={studentUi.error}>{attempt.start.error.message}</p>
        <button type="button" onClick={onExit} className={`mt-4 ${studentUi.quizGhostButton}`}><ArrowLeft size={16} aria-hidden="true" />Quay lại</button>
      </div>
    )
  }
  if (!ready) return <StudentLoadingState title="Đang chuẩn bị bài làm..." />

  return (
    <div className={studentUi.quizShell}>
      <div className={studentUi.quizTopBar}>
        <div className="flex flex-wrap items-center gap-3">
          <button type="button" onClick={onExit} aria-label="Thoát và quay lại tổng quan quiz" className="grid size-10 cursor-pointer place-items-center rounded-xl text-student-primary-strong outline-none hover:bg-student-surface-muted focus-visible:ring-4 focus-visible:ring-student-primary/15"><ArrowLeft size={20} /></button>
          <div className="min-w-0 flex-1">
            <p className="truncate font-student-display text-base font-bold text-student-ink">{quiz.title}</p>
            <p className={studentUi.caption} aria-live="polite">Đã trả lời {answered}/{questions.length}{attempt.resumed ? ` · Đã khôi phục bài làm dở${attempt.attempt?.updatedAtUtc ? ` (${formatRelativeTime(attempt.attempt.updatedAtUtc)})` : ''}` : ''}</p>
          </div>
          <QuizSaveStatus saveState={attempt.saveState} lastSavedAt={attempt.lastSavedAt} onRetry={attempt.retrySave} />
          <button type="button" onClick={() => setConfirming(true)} className={studentUi.quizPrimaryButton}><Send size={16} aria-hidden="true" />{QUIZ_COPY.submit}</button>
        </div>
        <div className="mt-3 flex gap-1" role="progressbar" aria-label="Tiến độ trả lời" aria-valuemin={0} aria-valuemax={questions.length} aria-valuenow={answered}>
          {questions.map((item) => <span key={item.id} className={`${studentUi.quizSegment} ${(attempt.answers[item.id]?.length ?? 0) > 0 ? studentUi.quizSegmentDone : ''}`} />)}
        </div>
      </div>

      <div className="grid gap-5 lg:grid-cols-[minmax(0,1fr)_15rem] lg:items-start">
        <div className="min-w-0">
          <QuestionCard
            key={question.id}
            question={question}
            index={currentIndex}
            total={questions.length}
            selectedIds={attempt.answers[question.id] ?? []}
            onSelect={(optionId) => attempt.select(question, optionId)}
            flagged={flagged.has(question.id)}
            onToggleFlag={toggleFlag}
          />
        </div>
        <aside className={`${studentUi.card} order-first p-4 lg:sticky lg:top-24 lg:order-none`}>
          <p className={studentUi.label}>Danh sách câu</p>
          <QuestionNavigator className="mt-3" questions={questions} answers={attempt.answers} currentIndex={currentIndex} flagged={flagged} onGo={goTo} />
          <p className={`mt-4 hidden lg:block ${studentUi.caption}`}><Keyboard size={13} className="mr-1 inline" aria-hidden="true" />{QUIZ_COPY.keyboardHint}</p>
        </aside>
      </div>

      <div className={studentUi.quizActionBar}>
        <button type="button" disabled={currentIndex === 0} onClick={() => goTo(currentIndex - 1)} className={studentUi.quizGhostButton}><ArrowLeft size={16} aria-hidden="true" />Câu trước</button>
        {currentIndex === questions.length - 1
          ? <button type="button" onClick={() => setConfirming(true)} className={studentUi.quizPrimaryButton}><Send size={16} aria-hidden="true" />{QUIZ_COPY.submit}</button>
          : <button type="button" onClick={() => goTo(currentIndex + 1)} className={studentUi.quizPrimaryButton}>Câu sau<ArrowRight size={16} aria-hidden="true" /></button>}
      </div>

      <QuizSubmitDialog
        open={confirming}
        total={questions.length}
        answered={answered}
        blankIndexes={blankIndexes}
        submitting={attempt.submit.submitting}
        error={attempt.submit.error}
        onGo={(index) => { goTo(index); setConfirming(false) }}
        onCancel={() => setConfirming(false)}
        onConfirm={() => void confirmSubmit()}
      />
    </div>
  )
}
