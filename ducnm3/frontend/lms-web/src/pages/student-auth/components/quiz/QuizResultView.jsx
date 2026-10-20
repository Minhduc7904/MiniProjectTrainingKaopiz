import { useEffect, useMemo, useState } from 'react'
import { ArrowRight, CheckCircle2, History, RotateCcw, XCircle, CircleDashed } from 'lucide-react'
import { studentUi } from '@/theme/student'
import { formatPoints, scorePercent, summarizeResult } from '@/utils/quizAnswers'
import { QuestionCard } from './QuestionCard'
import { QuestionNavigator } from './QuestionNavigator'

const RING_RADIUS = 52
const RING_LENGTH = 2 * Math.PI * RING_RADIUS

function verdict(percent) {
  if (percent === 100) return 'Hoàn hảo! Bạn trả lời đúng tất cả.'
  if (percent >= 80) return 'Rất tốt! Bạn nắm vững nội dung bài học.'
  if (percent >= 50) return 'Khá ổn. Xem lại các câu chưa đúng để chắc hơn.'
  return 'Đừng nản. Xem lại bài học rồi làm lại để cải thiện nhé.'
}

function ScoreRing({ percent }) {
  const [shown, setShown] = useState(false)
  useEffect(() => { const frame = requestAnimationFrame(() => setShown(true)); return () => cancelAnimationFrame(frame) }, [])

  return (
    <svg viewBox="0 0 120 120" className="size-36 shrink-0 -rotate-90" role="img" aria-label={`${percent}% số điểm`}>
      <circle cx="60" cy="60" r={RING_RADIUS} fill="none" strokeWidth="10" className="stroke-student-primary/15" />
      <circle cx="60" cy="60" r={RING_RADIUS} fill="none" strokeWidth="10" strokeLinecap="round" className="student-quiz-ring stroke-student-primary" strokeDasharray={RING_LENGTH} strokeDashoffset={shown ? RING_LENGTH * (1 - percent / 100) : RING_LENGTH} />
    </svg>
  )
}

/** Kết quả một lần nộp: điểm, thống kê đúng/sai/bỏ trống và duyệt từng câu (đáp án đã chọn so với đáp án đúng). */
export function QuizResultView({ quiz, result, onRetake, onHistory, onNextLesson }) {
  const [filter, setFilter] = useState('all')
  const [scrollTarget, setScrollTarget] = useState(null)
  const percent = scorePercent(result.score, result.maxScore)
  const summary = summarizeResult(result)
  const grades = useMemo(() => Object.fromEntries(result.questions.map((grade) => [grade.questionId, grade])), [result.questions])
  const answers = useMemo(() => Object.fromEntries(result.questions.map((grade) => [grade.questionId, grade.selectedOptionIds])), [result.questions])
  useEffect(() => {
    if (scrollTarget) document.getElementById(`review-${scrollTarget.id}`)?.scrollIntoView?.({ block: 'start', behavior: 'smooth' })
  }, [scrollTarget])

  const visible = quiz.questions.map((question, index) => ({ question, index })).filter(({ question }) => filter === 'all' || !grades[question.id]?.correct)

  return (
    <div className={`${studentUi.quizShell} student-enter`}>
      <section className={studentUi.quizHero} aria-live="polite">
        <div className="flex flex-col items-center gap-6 sm:flex-row">
          <div className="relative grid place-items-center">
            <ScoreRing percent={percent} />
            <p className="absolute font-student-display text-3xl font-bold text-student-primary-strong">{percent}%</p>
          </div>
          <div className="min-w-0 text-center sm:text-left">
            <p className={studentUi.eyebrow}>Kết quả · {quiz.title}</p>
            <h2 className="mt-1 font-student-display text-3xl font-bold text-student-ink">{formatPoints(result.score)}/{formatPoints(result.maxScore)} điểm</h2>
            <p className={`mt-2 ${studentUi.body}`}>{verdict(percent)}</p>
            <div className="mt-4 flex flex-wrap justify-center gap-2 sm:justify-start">
              <span className={studentUi.quizChipSuccess}><CheckCircle2 size={14} aria-hidden="true" />{summary.correct} đúng</span>
              <span className={studentUi.quizChipDanger}><XCircle size={14} aria-hidden="true" />{summary.incorrect} sai</span>
              <span className={studentUi.quizChipWarning}><CircleDashed size={14} aria-hidden="true" />{summary.unanswered} bỏ trống</span>
            </div>
          </div>
        </div>
        <div className="mt-6 flex flex-wrap gap-3 border-t border-student-line pt-5">
          <button type="button" onClick={onRetake} className={studentUi.quizPrimaryButton}><RotateCcw size={16} aria-hidden="true" />Làm lại</button>
          <button type="button" onClick={onHistory} className={studentUi.quizGhostButton}><History size={16} aria-hidden="true" />Các lần làm</button>
          {onNextLesson ? <button type="button" onClick={onNextLesson} className={studentUi.quizGhostButton}>Bài tiếp theo<ArrowRight size={16} aria-hidden="true" /></button> : null}
        </div>
      </section>

      <div className="mt-6 flex flex-wrap items-center justify-between gap-3">
        <h3 className="font-student-display text-xl font-bold text-student-ink">Xem lại bài làm</h3>
        <div role="group" aria-label="Lọc câu hỏi" className="flex gap-2">
          {[['all', 'Tất cả'], ['wrong', 'Chưa đúng']].map(([id, label]) => (
            <button key={id} type="button" aria-pressed={filter === id} onClick={() => setFilter(id)} className={`min-h-9 cursor-pointer rounded-xl px-3 text-sm font-bold outline-none focus-visible:ring-4 focus-visible:ring-student-primary/20 ${filter === id ? 'bg-student-primary text-white' : 'bg-student-surface text-student-muted hover:bg-student-surface-muted'}`}>{label}</button>
          ))}
        </div>
      </div>
      <QuestionNavigator className="mt-4" questions={quiz.questions} grades={grades} currentIndex={-1} onGo={(index) => { setFilter('all'); setScrollTarget({ id: quiz.questions[index].id }) }} />

      <div className="mt-5 grid gap-5">
        {visible.length === 0 ? <p className={studentUi.body}>Bạn đã trả lời đúng tất cả các câu.</p> : null}
        {visible.map(({ question, index }) => (
          <div key={question.id} id={`review-${question.id}`} className="scroll-mt-24">
            <QuestionCard question={question} index={index} total={quiz.questions.length} selectedIds={answers[question.id] ?? []} grade={grades[question.id]} />
          </div>
        ))}
      </div>
    </div>
  )
}
