import { Flag } from 'lucide-react'
import { RenderedMarkdown } from '@/components/markdown/RenderedMarkdown'
import { QUESTION_TYPE, QUESTION_TYPE_LABELS } from '@/constants/quiz'
import { studentUi } from '@/theme/student'
import { formatPoints } from '@/utils/quizAnswers'
import { OptionChoice } from './OptionChoice'

function reviewStateOf(option, selectedIds, correctIds) {
  const selected = selectedIds.includes(option.id)
  const correct = correctIds.includes(option.id)
  if (selected) return correct ? 'correct' : 'wrong'
  return correct ? 'missed' : 'neutral'
}

/**
 * Thẻ câu hỏi dùng cho cả lúc làm bài (`grade` rỗng) và xem lại kết quả (`grade` là kết quả chấm của câu này).
 */
export function QuestionCard({ question, index, total, selectedIds = [], onSelect, flagged = false, onToggleFlag, grade = null }) {
  const isMultiple = question.questionType === QUESTION_TYPE.multiple
  const correctIds = grade?.correctOptionIds ?? []

  return (
    <fieldset className={`${studentUi.quizQuestionCard} student-enter min-w-0`} aria-describedby={`question-hint-${question.id}`}>
      <legend className="sr-only">Câu {index + 1}</legend>
      <div className="flex flex-wrap items-center gap-2">
        <span className={studentUi.quizChip}>Câu {index + 1}/{total}</span>
        <span className={studentUi.quizChipQuiet}>{grade ? `${formatPoints(grade.pointsEarned)}/${formatPoints(grade.pointsPossible)} điểm` : `${formatPoints(question.points)} điểm`}</span>
        <span id={`question-hint-${question.id}`} className={studentUi.quizChipQuiet}>{QUESTION_TYPE_LABELS[question.questionType]}</span>
        {grade ? <span className={grade.correct ? studentUi.quizChipSuccess : grade.answered ? studentUi.quizChipDanger : studentUi.quizChipWarning}>{grade.correct ? 'Đúng' : grade.answered ? 'Chưa đúng' : 'Bỏ trống'}</span> : null}
        {onToggleFlag ? (
          <button
            type="button"
            aria-pressed={flagged}
            onClick={onToggleFlag}
            className={`ml-auto inline-flex min-h-9 cursor-pointer items-center gap-1.5 rounded-xl px-3 text-xs font-bold outline-none focus-visible:ring-4 focus-visible:ring-student-primary/20 ${flagged ? 'bg-student-bookmark/25 text-student-ink' : 'text-student-muted hover:bg-student-surface-muted'}`}
          >
            <Flag size={14} aria-hidden="true" />{flagged ? 'Đã đánh dấu xem lại' : 'Đánh dấu xem lại'}
          </button>
        ) : null}
      </div>

      <RenderedMarkdown className={`mt-5 ${studentUi.quizMarkdown} text-[16px] font-semibold`} html={question.questionHtml} emptyLabel="Câu hỏi chưa có nội dung." />
      <p className={`mt-3 ${studentUi.caption}`}>{isMultiple ? 'Chọn tất cả đáp án đúng. Chỉ được điểm khi chọn đủ và không chọn thừa.' : 'Chọn một đáp án.'}</p>

      <div className="mt-5 grid gap-3" role={isMultiple ? 'group' : 'radiogroup'} aria-label={`Đáp án câu ${index + 1}`}>
        {question.options.map((option, optionIndex) => (
          <OptionChoice
            key={option.id}
            question={question}
            option={option}
            index={optionIndex}
            name={`question-${question.id}`}
            checked={selectedIds.includes(option.id)}
            reviewState={grade ? reviewStateOf(option, selectedIds, correctIds) : null}
            onSelect={onSelect}
          />
        ))}
      </div>
    </fieldset>
  )
}
