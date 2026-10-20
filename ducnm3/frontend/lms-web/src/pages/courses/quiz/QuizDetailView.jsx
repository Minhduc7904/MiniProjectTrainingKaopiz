import { CheckCircle2, Pencil, Trash2 } from 'lucide-react'
import { RenderedMarkdown } from '@/components/markdown/RenderedMarkdown'
import { Button } from '@/components/ui/admin/Button'
import { Icon } from '@/components/ui/admin/Icon'
import { QUESTION_TYPE_LABELS, QUIZ_OPTION_LABELS, QUIZ_STATUS, QUIZ_STATUS_LABELS } from '@/constants/quiz'
import { adminUi } from '@/theme/admin'

/** Xem quiz đã lưu: Markdown đã render phía server, đáp án đúng được đánh dấu bằng icon và chữ. */
export function QuizDetailView({ quiz, onEdit, onDelete }) {
  return (
    <div>
      <div className="flex flex-wrap items-start justify-between gap-3">
        <div className="min-w-0">
          <p className={`text-[11px] font-medium tracking-[0.18em] uppercase ${adminUi.eyebrow}`}>Quiz</p>
          <h3 className={`mt-1 font-display text-[20px] font-semibold ${adminUi.title}`}>{quiz.title}</h3>
          <p className={`mt-1 flex flex-wrap items-center gap-2 text-[12px] ${adminUi.caption}`}>
            <span className={`rounded-full px-2 py-0.5 ${quiz.status === QUIZ_STATUS.published ? adminUi.statusPublished : adminUi.statusDraft}`}>{QUIZ_STATUS_LABELS[quiz.status]}</span>
            {quiz.questionCount} câu · tổng {quiz.totalPoints} điểm{quiz.hasAttempts ? ' · đã có bài làm' : ''}
          </p>
        </div>
        <div className="flex shrink-0 gap-2">
          <Button type="button" size="sm" variant="ghost" onClick={onEdit}><Icon icon={Pencil} size={15} />Sửa</Button>
          <Button type="button" size="sm" variant="danger" onClick={onDelete}><Icon icon={Trash2} size={15} />Xóa quiz</Button>
        </div>
      </div>

      <RenderedMarkdown className={`mt-4 ${adminUi.markdown}`} html={quiz.descriptionHtml} emptyLabel="Quiz chưa có mô tả." />

      <ol className="mt-6 grid gap-4">
        {quiz.questions.map((question, index) => (
          <li key={question.id} className={`rounded-lg p-4 ${adminUi.card}`}>
            <p className={`text-[12px] ${adminUi.caption}`}>Câu {index + 1} · {QUESTION_TYPE_LABELS[question.questionType]} · {question.points} điểm</p>
            <RenderedMarkdown className={`mt-2 ${adminUi.markdown}`} html={question.questionHtml} emptyLabel="Chưa có nội dung." />
            <ul className="mt-3 grid gap-2">
              {question.options.map((option, optionIndex) => (
                <li key={option.id} className={`flex items-start gap-2 rounded-md px-3 py-2 ${option.isCorrect ? adminUi.choiceActive : adminUi.choiceIdle}`}>
                  <span className={`font-display text-[13px] font-semibold ${adminUi.title}`}>{QUIZ_OPTION_LABELS[optionIndex]}.</span>
                  <RenderedMarkdown className={`min-w-0 flex-1 ${adminUi.markdown}`} html={option.optionHtml} emptyLabel="—" />
                  {option.isCorrect ? <span className={`inline-flex shrink-0 items-center gap-1 text-[12px] font-medium ${adminUi.accentText}`}><Icon icon={CheckCircle2} size={14} />Đáp án đúng</span> : null}
                </li>
              ))}
            </ul>
          </li>
        ))}
      </ol>
    </div>
  )
}
