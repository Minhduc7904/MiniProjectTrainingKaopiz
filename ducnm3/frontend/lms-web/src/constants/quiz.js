export const QUIZ_STATUS = { draft: 'DRAFT', published: 'PUBLISHED' }
export const QUIZ_STATUS_LABELS = { DRAFT: 'Bản nháp', PUBLISHED: 'Đã xuất bản' }

export const QUESTION_TYPE = { single: 'SINGLE_CHOICE', multiple: 'MULTIPLE_CHOICE' }
export const QUESTION_TYPE_LABELS = { SINGLE_CHOICE: 'Chọn 1 đáp án', MULTIPLE_CHOICE: 'Chọn nhiều đáp án' }

export const ATTEMPT_STATUS = { inProgress: 'IN_PROGRESS', submitted: 'SUBMITTED' }

export const QUIZ_LIMITS = { titleMin: 2, titleMax: 200, markdownMax: 20000, optionMax: 2000, maxQuestions: 100, minOptions: 2, maxOptions: 10, maxPoints: 100 }

export const QUIZ_SAVE_STATE = { saved: 'saved', dirty: 'dirty', saving: 'saving', error: 'error' }

export const QUIZ_AUTOSAVE_DELAY_MS = 600
export const QUIZ_RETRY_DELAYS_MS = [1000, 2000, 4000, 8000, 15000]

export const QUIZ_OPTION_LABELS = 'ABCDEFGHIJ'

export const QUIZ_COPY = {
  noTimeLimit: 'Không giới hạn thời gian',
  autoSave: 'Tự động lưu khi làm',
  emptyTitle: 'Bài học này chưa có quiz',
  start: 'Bắt đầu làm bài',
  resume: 'Tiếp tục làm bài',
  retake: 'Làm lại',
  submit: 'Nộp bài',
  keyboardHint: '← → đổi câu · 1–9 hoặc A–F chọn đáp án · Ctrl/⌘ + Enter nộp bài',
}
