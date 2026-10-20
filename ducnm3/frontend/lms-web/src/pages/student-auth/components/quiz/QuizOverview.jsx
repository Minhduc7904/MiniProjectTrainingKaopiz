import { Clock, History, ListChecks, Play, RotateCcw, Save, Trophy } from 'lucide-react'
import { RenderedMarkdown } from '@/components/markdown/RenderedMarkdown'
import { ATTEMPT_STATUS, QUIZ_COPY } from '@/constants/quiz'
import { studentUi } from '@/theme/student'
import { formatPoints, formatRelativeTime, scorePercent } from '@/utils/quizAnswers'

function AttemptRow({ attempt, total, onOpen }) {
  const inProgress = attempt.status === ATTEMPT_STATUS.inProgress
  return (
    <li>
      <button type="button" onClick={() => onOpen(attempt)} className="student-press flex min-h-14 w-full cursor-pointer items-center gap-3 rounded-2xl border border-student-line bg-student-surface px-4 py-3 text-left outline-none hover:bg-student-surface-muted focus-visible:ring-4 focus-visible:ring-student-primary/15">
        <span className="min-w-0 flex-1">
          <span className="block font-student-display text-sm font-bold text-student-ink">
            {inProgress ? 'Đang làm dở' : `${formatPoints(attempt.score)}/${formatPoints(attempt.maxScore)} điểm · ${scorePercent(attempt.score, attempt.maxScore)}%`}
          </span>
          <span className={studentUi.caption}>
            {inProgress ? `Đã trả lời ${attempt.answeredQuestions}/${total} · lưu ${formatRelativeTime(attempt.updatedAtUtc)}` : `Nộp ${formatRelativeTime(attempt.submittedAtUtc)}`}
          </span>
        </span>
        <span className={inProgress ? studentUi.quizChipWarning : studentUi.quizChipSuccess}>{inProgress ? 'Tiếp tục' : 'Xem lại'}</span>
      </button>
    </li>
  )
}

/** Tổng quan quiz: mô tả, thông tin, điểm cao nhất, các lần làm và nút hành động chính theo trạng thái. */
export function QuizOverview({ quiz, attempts, historyLoading, onStart, onOpenAttempt }) {
  const open = attempts.find((attempt) => attempt.status === ATTEMPT_STATUS.inProgress)
  const submitted = attempts.filter((attempt) => attempt.status === ATTEMPT_STATUS.submitted)
  const best = submitted.reduce((top, attempt) => (!top || attempt.score > top.score ? attempt : top), null)
  const cta = open
    ? { label: `${QUIZ_COPY.resume} (đã trả lời ${open.answeredQuestions}/${quiz.questions.length})`, icon: Play }
    : submitted.length
      ? { label: QUIZ_COPY.retake, icon: RotateCcw }
      : { label: QUIZ_COPY.start, icon: Play }

  return (
    <div className={`${studentUi.quizShell} student-enter`}>
      <section className={studentUi.quizHero}>
        <p className={studentUi.eyebrow}>Bài trắc nghiệm</p>
        <h1 className={`mt-2 ${studentUi.title}`}>{quiz.title}</h1>
        <RenderedMarkdown className={`mt-4 ${studentUi.learningMarkdown}`} html={quiz.descriptionHtml} emptyLabel="" />
        <div className="mt-5 flex flex-wrap gap-2">
          <span className={studentUi.quizChip}><ListChecks size={14} aria-hidden="true" />{quiz.questionCount} câu hỏi</span>
          <span className={studentUi.quizChip}><Trophy size={14} aria-hidden="true" />Tổng {formatPoints(quiz.totalPoints)} điểm</span>
          <span className={studentUi.quizChipQuiet}><Clock size={14} aria-hidden="true" />{QUIZ_COPY.noTimeLimit}</span>
          <span className={studentUi.quizChipQuiet}><Save size={14} aria-hidden="true" />{QUIZ_COPY.autoSave}</span>
        </div>
        {best ? (
          <p className="mt-5 rounded-2xl bg-student-complete/10 px-4 py-3 text-sm font-semibold text-student-ink">
            Điểm cao nhất của bạn: <strong>{formatPoints(best.score)}/{formatPoints(best.maxScore)}</strong> ({scorePercent(best.score, best.maxScore)}%)
          </p>
        ) : null}
        {open ? <p className={`mt-3 ${studentUi.caption}`} role="status">Đã khôi phục bài làm dở lúc {formatRelativeTime(open.updatedAtUtc)}.</p> : null}
        <button type="button" onClick={onStart} className={`mt-6 min-h-12 px-6 ${studentUi.quizPrimaryButton}`}><cta.icon size={18} aria-hidden="true" />{cta.label}</button>
      </section>

      <section className="mt-6" aria-labelledby="quiz-attempt-history">
        <h2 id="quiz-attempt-history" className="flex items-center gap-2 font-student-display text-lg font-bold text-student-ink"><History size={18} aria-hidden="true" />Các lần làm</h2>
        {historyLoading ? <p className={`mt-3 ${studentUi.caption}`}>Đang tải lịch sử...</p> : null}
        {!historyLoading && attempts.length === 0 ? <p className={`mt-3 ${studentUi.body}`}>Bạn chưa làm bài này lần nào.</p> : null}
        <ul className="mt-3 grid gap-2">
          {attempts.map((attempt) => <AttemptRow key={attempt.id} attempt={attempt} total={quiz.questions.length} onOpen={onOpenAttempt} />)}
        </ul>
      </section>
    </div>
  )
}
