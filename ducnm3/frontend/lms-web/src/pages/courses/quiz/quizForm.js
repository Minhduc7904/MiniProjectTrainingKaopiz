import { QUESTION_TYPE, QUIZ_LIMITS, QUIZ_STATUS } from '@/constants/quiz'

let keySeed = 0
const nextKey = () => `quiz-key-${++keySeed}`

export const emptyOption = (isCorrect = false) => ({ key: nextKey(), optionMarkdown: '', isCorrect })

export const emptyQuestion = () => ({
  key: nextKey(),
  questionMarkdown: '',
  questionType: QUESTION_TYPE.single,
  points: '1',
  options: [emptyOption(true), emptyOption()],
})

export const emptyQuiz = () => ({ title: '', descriptionMarkdown: '', status: QUIZ_STATUS.draft, questions: [emptyQuestion()] })

/** Chuyển chi tiết quiz từ API (admin) thành state form có `key` ổn định để React render danh sách. */
export function fromQuizDetail(detail) {
  return {
    title: detail.title,
    descriptionMarkdown: detail.descriptionMarkdown ?? '',
    status: detail.status,
    questions: detail.questions.map((question) => ({
      key: nextKey(),
      questionMarkdown: question.questionMarkdown,
      questionType: question.questionType,
      points: String(question.points),
      options: question.options.map((option) => ({ key: nextKey(), optionMarkdown: option.optionMarkdown, isCorrect: option.isCorrect })),
    })),
  }
}

export function parsePoints(value) {
  const text = String(value ?? '').trim().replace(',', '.')
  if (!text || !/^\d+(\.\d{1,2})?$/.test(text)) return null
  const number = Number(text)
  return number > 0 && number <= QUIZ_LIMITS.maxPoints ? number : null
}

/** `includeQuestions` false chỉ gửi tiêu đề/mô tả/trạng thái, khi đó server giữ nguyên cây câu hỏi (bắt buộc khi quiz đã có bài làm). */
export function toQuizPayload(form, { includeQuestions = true } = {}) {
  const payload = {
    title: form.title.trim(),
    descriptionMarkdown: form.descriptionMarkdown.trim() || null,
    status: form.status,
  }
  if (!includeQuestions) return payload
  return {
    ...payload,
    questions: form.questions.map((question) => ({
      questionMarkdown: question.questionMarkdown.trim(),
      questionType: question.questionType,
      points: parsePoints(question.points),
      options: question.options.map((option) => ({ optionMarkdown: option.optionMarkdown.trim(), isCorrect: option.isCorrect })),
    })),
  }
}

export function totalPoints(form) {
  return form.questions.reduce((sum, question) => sum + (parsePoints(question.points) ?? 0), 0)
}

/** Cùng luật với server (QuizValidation) để báo lỗi ngay khi soạn; server vẫn là nơi quyết định cuối cùng. */
export function validateQuizForm(form, { includeQuestions = true } = {}) {
  const errors = { title: null, status: null, questions: {}, count: 0 }
  const add = (target, key, message) => { target[key] = message; errors.count += 1 }
  const title = form.title.trim()
  if (title.length < QUIZ_LIMITS.titleMin || title.length > QUIZ_LIMITS.titleMax) {
    add(errors, 'title', `Tiêu đề dài ${QUIZ_LIMITS.titleMin}–${QUIZ_LIMITS.titleMax} ký tự.`)
  }
  if (form.descriptionMarkdown.length > QUIZ_LIMITS.markdownMax) add(errors, 'description', `Mô tả tối đa ${QUIZ_LIMITS.markdownMax} ký tự.`)
  if (!includeQuestions) return errors

  if (form.status === QUIZ_STATUS.published && form.questions.length === 0) add(errors, 'status', 'Cần ít nhất 1 câu hỏi để xuất bản.')
  if (form.questions.length > QUIZ_LIMITS.maxQuestions) add(errors, 'questionCount', `Tối đa ${QUIZ_LIMITS.maxQuestions} câu hỏi.`)

  form.questions.forEach((question) => {
    const found = {}
    const markdown = question.questionMarkdown.trim()
    if (!markdown || markdown.length > QUIZ_LIMITS.markdownMax) add(found, 'questionMarkdown', 'Nội dung câu hỏi là bắt buộc.')
    if (parsePoints(question.points) === null) add(found, 'points', `Điểm lớn hơn 0, tối đa ${QUIZ_LIMITS.maxPoints}, 2 chữ số thập phân.`)
    if (question.options.length < QUIZ_LIMITS.minOptions || question.options.length > QUIZ_LIMITS.maxOptions) {
      add(found, 'options', `Mỗi câu cần ${QUIZ_LIMITS.minOptions}–${QUIZ_LIMITS.maxOptions} lựa chọn.`)
    }
    const correct = question.options.filter((option) => option.isCorrect).length
    if (question.questionType === QUESTION_TYPE.single && correct !== 1) add(found, 'correct', 'Câu chọn 1 cần đúng 1 đáp án đúng.')
    if (question.questionType === QUESTION_TYPE.multiple && correct < 1) add(found, 'correct', 'Câu chọn nhiều cần ít nhất 1 đáp án đúng.')
    question.options.forEach((option) => {
      const text = option.optionMarkdown.trim()
      if (!text || text.length > QUIZ_LIMITS.optionMax) add(found, `option:${option.key}`, 'Nội dung lựa chọn là bắt buộc.')
    })
    if (Object.keys(found).length) errors.questions[question.key] = found
  })
  return errors
}

/** Chọn 1: đánh dấu duy nhất một đáp án đúng; chọn nhiều: bật/tắt từng đáp án. */
export function markCorrect(question, optionKey) {
  const options = question.options.map((option) => {
    if (question.questionType === QUESTION_TYPE.single) return { ...option, isCorrect: option.key === optionKey }
    return option.key === optionKey ? { ...option, isCorrect: !option.isCorrect } : option
  })
  return { ...question, options }
}

/** Đổi loại câu hỏi; khi về "chọn 1" chỉ giữ lại đáp án đúng đầu tiên. */
export function changeQuestionType(question, questionType) {
  if (question.questionType === questionType) return question
  if (questionType === QUESTION_TYPE.multiple) return { ...question, questionType }
  const firstCorrect = question.options.find((option) => option.isCorrect)?.key ?? question.options[0]?.key
  return { ...question, questionType, options: question.options.map((option) => ({ ...option, isCorrect: option.key === firstCorrect })) }
}

export function moveItem(list, from, to) {
  if (to < 0 || to >= list.length || from === to) return list
  const next = [...list]
  const [item] = next.splice(from, 1)
  next.splice(to, 0, item)
  return next
}
