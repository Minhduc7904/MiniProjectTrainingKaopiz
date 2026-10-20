import { ArrowDown, ArrowUp, Plus, Trash2 } from 'lucide-react'
import { Button } from '@/components/ui/admin/Button'
import { Dropdown } from '@/components/ui/admin/Dropdown'
import { Icon } from '@/components/ui/admin/Icon'
import { QUESTION_TYPE, QUESTION_TYPE_LABELS, QUIZ_LIMITS, QUIZ_OPTION_LABELS } from '@/constants/quiz'
import { adminUi } from '@/theme/admin'
import { QuizMarkdownField } from './QuizMarkdownField'
import { changeQuestionType, emptyOption, markCorrect } from './quizForm'

const TYPE_OPTIONS = Object.values(QUESTION_TYPE).map((value) => ({ value, label: QUESTION_TYPE_LABELS[value] }))

/** Soạn một câu hỏi: nội dung, loại, điểm riêng và danh sách lựa chọn kèm đánh dấu đáp án đúng. */
export function QuestionEditor({ question, index, count, errors = {}, onChange, onRemove, onMove, onPickMedia }) {
  const isSingle = question.questionType === QUESTION_TYPE.single
  const update = (patch) => onChange({ ...question, ...patch })
  const updateOption = (key, patch) => update({ options: question.options.map((option) => (option.key === key ? { ...option, ...patch } : option)) })

  return (
    <article className={`rounded-lg p-4 ${adminUi.card}`} aria-label={`Câu ${index + 1}`}>
      <div className="flex flex-wrap items-center gap-2">
        <span className={`rounded-full px-2.5 py-0.5 text-[12px] font-medium ${adminUi.badgeMuted}`}>Câu {index + 1}</span>
        <div className="w-48"><Dropdown id={`type-${question.key}`} value={question.questionType} options={TYPE_OPTIONS} onChange={(value) => onChange(changeQuestionType(question, value))} /></div>
        <label className={`flex items-center gap-2 text-[12px] ${adminUi.body}`}>
          Điểm
          <input
            aria-label={`Điểm câu ${index + 1}`}
            inputMode="decimal"
            value={question.points}
            onChange={(event) => update({ points: event.target.value })}
            aria-invalid={Boolean(errors.points)}
            className={`${adminUi.control} h-9 w-20 ${errors.points ? adminUi.controlInvalid : ''}`}
          />
        </label>
        <div className="ml-auto flex items-center gap-1">
          <Button type="button" size="icon" variant="ghost" aria-label={`Chuyển câu ${index + 1} lên`} disabled={index === 0} onClick={() => onMove(-1)}><Icon icon={ArrowUp} /></Button>
          <Button type="button" size="icon" variant="ghost" aria-label={`Chuyển câu ${index + 1} xuống`} disabled={index === count - 1} onClick={() => onMove(1)}><Icon icon={ArrowDown} /></Button>
          <Button type="button" size="icon" variant="ghost" aria-label={`Xóa câu ${index + 1}`} onClick={onRemove}><Icon icon={Trash2} /></Button>
        </div>
      </div>
      {errors.points ? <p role="alert" className={`mt-1 text-[12px] ${adminUi.dangerText}`}>{errors.points}</p> : null}

      <div className="mt-3">
        <QuizMarkdownField id={`question-${question.key}`} label="Nội dung câu hỏi" rows={4} value={question.questionMarkdown} error={errors.questionMarkdown} onChange={(value) => update({ questionMarkdown: value })} onPickMedia={onPickMedia} />
      </div>

      <div className="mt-4">
        <p className={`text-[12px] font-medium ${adminUi.body}`}>Lựa chọn · {isSingle ? 'chọn 1 đáp án đúng' : 'tick tất cả đáp án đúng'}</p>
        <div className="mt-2 grid gap-3">
          {question.options.map((option, optionIndex) => (
            <div key={option.key} className="flex items-start gap-3">
              <label className="mt-6 flex cursor-pointer items-center" title="Đáp án đúng">
                <input
                  type={isSingle ? 'radio' : 'checkbox'}
                  name={`correct-${question.key}`}
                  checked={option.isCorrect}
                  aria-label={`Đáp án ${QUIZ_OPTION_LABELS[optionIndex]} là đáp án đúng`}
                  onChange={() => onChange(markCorrect(question, option.key))}
                  className="size-4 cursor-pointer"
                />
              </label>
              <div className="min-w-0 flex-1">
                <QuizMarkdownField id={`option-${option.key}`} label={`Lựa chọn ${QUIZ_OPTION_LABELS[optionIndex]}`} rows={2} value={option.optionMarkdown} error={errors[`option:${option.key}`]} onChange={(value) => updateOption(option.key, { optionMarkdown: value })} onPickMedia={onPickMedia} />
              </div>
              <Button type="button" size="icon" variant="ghost" aria-label={`Xóa lựa chọn ${QUIZ_OPTION_LABELS[optionIndex]}`} disabled={question.options.length <= QUIZ_LIMITS.minOptions} onClick={() => update({ options: question.options.filter((item) => item.key !== option.key) })}><Icon icon={Trash2} /></Button>
            </div>
          ))}
        </div>
        {errors.correct ? <p role="alert" className={`mt-2 text-[12px] ${adminUi.dangerText}`}>{errors.correct}</p> : null}
        {errors.options ? <p role="alert" className={`mt-2 text-[12px] ${adminUi.dangerText}`}>{errors.options}</p> : null}
        <div className="mt-3">
          <Button type="button" size="sm" variant="ghost" disabled={question.options.length >= QUIZ_LIMITS.maxOptions} onClick={() => update({ options: [...question.options, emptyOption()] })}><Icon icon={Plus} size={15} />Thêm lựa chọn</Button>
        </div>
      </div>
    </article>
  )
}
