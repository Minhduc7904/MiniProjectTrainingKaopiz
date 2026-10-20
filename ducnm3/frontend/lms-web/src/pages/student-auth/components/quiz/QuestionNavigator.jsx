import { Flag } from 'lucide-react'
import { studentUi } from '@/theme/student'
import { isAnswered } from '@/utils/quizAnswers'

function statusOf(question, answers, grades) {
  if (grades) {
    const grade = grades[question.id]
    if (grade?.correct) return { className: studentUi.quizNavCorrect, label: 'đúng' }
    if (grade?.answered) return { className: studentUi.quizNavWrong, label: 'chưa đúng' }
    return { className: studentUi.quizNavBlank, label: 'bỏ trống' }
  }
  return isAnswered(answers, question.id)
    ? { className: studentUi.quizNavAnswered, label: 'đã trả lời' }
    : { className: studentUi.quizNavIdle, label: 'chưa trả lời' }
}

/** Lưới số câu: đã trả lời (đặc), chưa trả lời (viền), câu hiện tại (vòng nhấn), đánh dấu xem lại (cờ). */
export function QuestionNavigator({ questions, answers = {}, grades = null, currentIndex, flagged, onGo, className = '' }) {
  return (
    <nav aria-label="Danh sách câu hỏi" className={className}>
      <ol className="flex flex-wrap gap-2 lg:grid lg:grid-cols-5">
        {questions.map((question, index) => {
          const status = statusOf(question, answers, grades)
          return (
            <li key={question.id} className="relative">
              <button
                type="button"
                aria-label={`Câu ${index + 1}, ${status.label}${flagged?.has(question.id) ? ', đã đánh dấu xem lại' : ''}`}
                aria-current={index === currentIndex ? 'step' : undefined}
                onClick={() => onGo(index)}
                className={`${studentUi.quizNavButton} ${status.className} ${index === currentIndex ? studentUi.quizNavCurrent : ''}`}
              >
                {index + 1}
              </button>
              {flagged?.has(question.id) ? <Flag size={12} aria-hidden="true" className="absolute -top-1 -right-1 fill-student-bookmark text-student-bookmark" /> : null}
            </li>
          )
        })}
      </ol>
    </nav>
  )
}
