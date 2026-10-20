import { useState } from 'react'
import { Lock, Plus, Save } from 'lucide-react'
import { MediaLibraryModal } from '@/components/media/MediaLibraryModal'
import { Button } from '@/components/ui/admin/Button'
import { Dropdown } from '@/components/ui/admin/Dropdown'
import { TextInput } from '@/components/ui/admin/Field'
import { Icon } from '@/components/ui/admin/Icon'
import { QUIZ_LIMITS, QUIZ_STATUS, QUIZ_STATUS_LABELS } from '@/constants/quiz'
import { adminUi } from '@/theme/admin'
import { QuestionEditor } from './QuestionEditor'
import { QuizMarkdownField } from './QuizMarkdownField'
import { emptyQuestion, moveItem, totalPoints, validateQuizForm } from './quizForm'

const STATUS_OPTIONS = Object.values(QUIZ_STATUS).map((value) => ({ value, label: QUIZ_STATUS_LABELS[value] }))

function insertAtCursor({ element, value, onChange }, media) {
  if (!media?.contentUrl) return
  const alt = (media.originalFileName || media.id).replaceAll(']', '')
  const snippet = `![${alt}](${media.contentUrl})`
  const start = element?.selectionStart ?? value.length
  const end = element?.selectionEnd ?? value.length
  onChange(`${value.slice(0, start)}${snippet}${value.slice(end)}`)
}

/**
 * Form soạn/sửa quiz. `structureLocked` (quiz đã có bài làm) chỉ cho đổi tiêu đề, mô tả, trạng thái vì server
 * trả 409 QUIZ_HAS_ATTEMPTS khi thay cây câu hỏi.
 */
export function QuizEditor({ form, onChange, structureLocked, saving, serverError, submitLabel, onSubmit, onCancel }) {
  const [showErrors, setShowErrors] = useState(false)
  const [picker, setPicker] = useState(null)
  const errors = validateQuizForm(form, { includeQuestions: !structureLocked })

  const submit = (event) => {
    event.preventDefault()
    setShowErrors(true)
    if (errors.count === 0) onSubmit()
  }
  const patch = (changes) => onChange({ ...form, ...changes })
  const updateQuestion = (key, next) => patch({ questions: form.questions.map((question) => (question.key === key ? next : question)) })

  return (
    <form onSubmit={submit} noValidate className="grid gap-5">
      {structureLocked ? (
        <p className={`flex items-start gap-2 rounded-md px-3 py-2 text-[13px] ${adminUi.badgeWarning}`}>
          <Icon icon={Lock} size={16} />
          <span>Quiz đã có học viên làm bài nên chỉ sửa được tiêu đề, mô tả và trạng thái. Muốn đổi câu hỏi, hãy tạo quiz mới hoặc xóa quiz này.</span>
        </p>
      ) : null}

      <div className="grid gap-4 sm:grid-cols-[minmax(0,1fr)_14rem]">
        <div>
          <label htmlFor="quiz-title" className={`text-[12px] font-medium ${adminUi.body}`}>Tiêu đề quiz</label>
          <div className="mt-1"><TextInput id="quiz-title" value={form.title} invalid={showErrors && Boolean(errors.title)} placeholder="Ví dụ: Kiểm tra cuối bài" onChange={(event) => patch({ title: event.target.value })} /></div>
          {showErrors && errors.title ? <p role="alert" className={`mt-1 text-[12px] ${adminUi.dangerText}`}>{errors.title}</p> : null}
        </div>
        <div>
          <p className={`text-[12px] font-medium ${adminUi.body}`}>Trạng thái</p>
          <div className="mt-1"><Dropdown id="quiz-status" value={form.status} options={STATUS_OPTIONS} onChange={(value) => patch({ status: value })} /></div>
          {showErrors && errors.status ? <p role="alert" className={`mt-1 text-[12px] ${adminUi.dangerText}`}>{errors.status}</p> : null}
        </div>
      </div>

      <QuizMarkdownField id="quiz-description" label="Mô tả (Markdown)" rows={4} value={form.descriptionMarkdown} error={showErrors ? errors.description : null} onChange={(value) => patch({ descriptionMarkdown: value })} onPickMedia={setPicker} />

      {structureLocked ? null : (
        <section aria-label="Danh sách câu hỏi" className="grid gap-4">
          <div className="flex flex-wrap items-center justify-between gap-2">
            <h4 className={`font-display text-[16px] font-semibold ${adminUi.title}`}>Câu hỏi ({form.questions.length}) · Tổng {totalPoints(form)} điểm</h4>
            <Button type="button" size="sm" variant="ghost" disabled={form.questions.length >= QUIZ_LIMITS.maxQuestions} onClick={() => patch({ questions: [...form.questions, emptyQuestion()] })}><Icon icon={Plus} size={15} />Thêm câu hỏi</Button>
          </div>
          {form.questions.map((question, index) => (
            <QuestionEditor
              key={question.key}
              question={question}
              index={index}
              count={form.questions.length}
              errors={showErrors ? errors.questions[question.key] : undefined}
              onChange={(next) => updateQuestion(question.key, next)}
              onRemove={() => patch({ questions: form.questions.filter((item) => item.key !== question.key) })}
              onMove={(delta) => patch({ questions: moveItem(form.questions, index, index + delta) })}
              onPickMedia={setPicker}
            />
          ))}
        </section>
      )}

      {showErrors && errors.count > 0 ? <p role="alert" className={`rounded-md px-3 py-2 text-[13px] ${adminUi.badgeDanger}`}>Còn {errors.count} lỗi cần sửa trước khi lưu.</p> : null}
      {serverError ? <p role="alert" className={`rounded-md px-3 py-2 text-[13px] ${adminUi.badgeDanger}`}>{serverError}</p> : null}

      <div className="flex justify-end gap-2">
        <Button type="button" variant="ghost" disabled={saving} onClick={onCancel}>Hủy</Button>
        <Button type="submit" disabled={saving}><Icon icon={Save} size={15} />{saving ? 'Đang lưu...' : submitLabel}</Button>
      </div>

      <MediaLibraryModal open={Boolean(picker)} selectionMode="single" onClose={() => setPicker(null)} onConfirm={(items) => { insertAtCursor(picker, items[0]); setPicker(null) }} />
    </form>
  )
}
