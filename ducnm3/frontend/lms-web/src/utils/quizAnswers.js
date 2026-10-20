import { QUESTION_TYPE } from '@/constants/quiz'

/** Chuyển đáp án từ server `[{questionId, optionIds}]` thành map `{ [questionId]: optionId[] }`. */
export function answersToMap(answers = []) {
  return Object.fromEntries(
    answers.filter((answer) => answer.optionIds?.length).map((answer) => [answer.questionId, [...answer.optionIds]]),
  )
}

/** Chuyển map về payload gửi server; câu bỏ trống vẫn được gửi để server thay thế bản nháp cũ. */
export function mapToPayload(questions, answerMap) {
  return questions.map((question) => ({ questionId: question.id, optionIds: answerMap[question.id] ?? [] }))
}

/** Chọn 1: thay thế; chọn nhiều: bật/tắt. Luôn trả map mới, không đổi map cũ. */
export function toggleOption(answerMap, question, optionId) {
  const current = answerMap[question.id] ?? []
  if (question.questionType === QUESTION_TYPE.single) {
    return current.length === 1 && current[0] === optionId ? answerMap : { ...answerMap, [question.id]: [optionId] }
  }
  const next = current.includes(optionId) ? current.filter((id) => id !== optionId) : [...current, optionId]
  if (!next.length) {
    const { [question.id]: _removed, ...rest } = answerMap
    return rest
  }
  return { ...answerMap, [question.id]: next }
}

export function isAnswered(answerMap, questionId) {
  return (answerMap[questionId]?.length ?? 0) > 0
}

export function countAnswered(questions, answerMap) {
  return questions.filter((question) => isAnswered(answerMap, question.id)).length
}

/** Chỉ số (0-based) của các câu chưa trả lời. */
export function unansweredIndexes(questions, answerMap) {
  return questions.flatMap((question, index) => (isAnswered(answerMap, question.id) ? [] : [index]))
}

export function scorePercent(score, maxScore) {
  return maxScore > 0 ? Math.round((score / maxScore) * 100) : 0
}

/** Số điểm hiển thị gọn: 5 -> "5", 5.5 -> "5,5", tối đa 2 chữ số thập phân. */
export function formatPoints(value) {
  if (value == null) return '—'
  return new Intl.NumberFormat('vi-VN', { maximumFractionDigits: 2 }).format(value)
}

export function formatRelativeTime(isoString, now = Date.now()) {
  if (!isoString) return ''
  const seconds = Math.max(0, Math.round((now - new Date(isoString).getTime()) / 1000))
  if (seconds < 45) return 'vừa xong'
  if (seconds < 3600) return `${Math.max(1, Math.round(seconds / 60))} phút trước`
  if (seconds < 86400) return `${Math.round(seconds / 3600)} giờ trước`
  return new Intl.DateTimeFormat('vi-VN', { dateStyle: 'medium', timeStyle: 'short' }).format(new Date(isoString))
}

/** Tổng hợp đúng/sai/bỏ trống từ kết quả chấm. */
export function summarizeResult(result) {
  const questions = result?.questions ?? []
  return {
    correct: questions.filter((question) => question.correct).length,
    incorrect: questions.filter((question) => question.answered && !question.correct).length,
    unanswered: questions.filter((question) => !question.answered).length,
  }
}
