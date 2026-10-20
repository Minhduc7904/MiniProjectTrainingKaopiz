import { ChevronRight, ListChecks } from 'lucide-react'
import { studentUi } from '@/theme/student'
import { quizStatusOf } from './quizStatus'
import { formatPoints } from '@/utils/quizAnswers'

/** Thẻ "Bài trắc nghiệm" dưới nội dung bài học, dẫn vào từng quiz của bài. */
export function LessonQuizCard({ quizzes, onOpen }) {
  if (!quizzes.length) return null
  return (
    <section className="mt-10 border-t border-student-line pt-6" aria-labelledby="lesson-quizzes-title">
      <h2 id="lesson-quizzes-title" className="flex items-center gap-2 font-student-display text-xl font-bold text-student-ink"><ListChecks size={20} aria-hidden="true" />Bài trắc nghiệm</h2>
      <ul className="mt-4 grid gap-3">
        {quizzes.map((quiz) => {
          const status = quizStatusOf(quiz)
          return (
            <li key={quiz.id}>
              <button type="button" onClick={() => onOpen(quiz.id)} className="student-press flex min-h-16 w-full cursor-pointer items-center gap-3 rounded-2xl border border-student-line bg-student-surface px-4 py-3 text-left outline-none hover:bg-student-surface-muted focus-visible:ring-4 focus-visible:ring-student-primary/15">
                <span className="min-w-0 flex-1">
                  <span className="block truncate font-student-display text-base font-bold text-student-ink">{quiz.title}</span>
                  <span className={studentUi.caption}>{quiz.questionCount} câu · {formatPoints(quiz.totalPoints)} điểm</span>
                </span>
                <span className={status.className}>{status.label}</span>
                <ChevronRight size={18} aria-hidden="true" className="text-student-subtle" />
              </button>
            </li>
          )
        })}
      </ul>
    </section>
  )
}
