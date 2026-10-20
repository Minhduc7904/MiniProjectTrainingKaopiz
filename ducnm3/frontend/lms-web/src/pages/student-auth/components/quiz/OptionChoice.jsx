import { Check, X } from 'lucide-react'
import { RenderedMarkdown } from '@/components/markdown/RenderedMarkdown'
import { QUESTION_TYPE, QUIZ_OPTION_LABELS } from '@/constants/quiz'
import { studentUi } from '@/theme/student'

const REVIEW_STYLES = {
  correct: studentUi.quizOptionCorrect,
  wrong: studentUi.quizOptionWrong,
  missed: studentUi.quizOptionMissed,
  neutral: studentUi.quizOptionNeutral,
}

const REVIEW_LABELS = {
  correct: { text: 'Bạn chọn · Đúng', className: studentUi.quizChipSuccess, icon: Check },
  wrong: { text: 'Bạn chọn · Sai', className: studentUi.quizChipDanger, icon: X },
  missed: { text: 'Đáp án đúng', className: studentUi.quizChipSuccess, icon: Check },
}

/**
 * Một lựa chọn. Input gốc (radio/checkbox) phủ toàn thẻ nên bàn phím, trình đọc màn hình và chạm đều dùng được;
 * ở chế độ xem lại (`reviewState`) input bị khóa và trạng thái được thể hiện bằng icon + chữ, không chỉ bằng màu.
 */
export function OptionChoice({ question, option, index, checked, name, reviewState = null, onSelect }) {
  const isMultiple = question.questionType === QUESTION_TYPE.multiple
  const label = QUIZ_OPTION_LABELS[index] ?? String(index + 1)
  const review = reviewState ? REVIEW_LABELS[reviewState] : null
  const stateClass = reviewState ? REVIEW_STYLES[reviewState] : checked ? studentUi.quizOptionSelected : studentUi.quizOptionIdle

  return (
    <div className="relative">
      <input
        type={isMultiple ? 'checkbox' : 'radio'}
        name={name}
        checked={checked}
        disabled={Boolean(reviewState)}
        aria-label={`Đáp án ${label}`}
        onChange={() => onSelect?.(option.id)}
        className="peer absolute inset-0 z-10 m-0 size-full cursor-pointer opacity-0 disabled:cursor-default"
      />
      <div className={`${studentUi.quizOption} ${stateClass} peer-focus-visible:ring-4 peer-focus-visible:ring-student-primary/25`}>
        <span aria-hidden="true" className={`${studentUi.quizOptionBadge} ${checked && !reviewState ? studentUi.quizOptionBadgeSelected : ''}`}>{label}</span>
        <RenderedMarkdown className={studentUi.quizMarkdown} html={option.optionHtml} emptyLabel="—" />
        {review ? (
          <span className={`${review.className} shrink-0`}><review.icon size={14} aria-hidden="true" />{review.text}</span>
        ) : (
          <span aria-hidden="true" className={`${studentUi.quizIndicator} ${isMultiple ? 'rounded-lg' : 'rounded-full'} ${checked ? `${studentUi.quizIndicatorSelected} student-pop` : ''}`}>
            {checked ? <Check size={14} strokeWidth={3} /> : null}
          </span>
        )}
      </div>
    </div>
  )
}
