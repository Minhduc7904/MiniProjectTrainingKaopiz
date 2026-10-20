import { describe, expect, it } from 'vitest'
import { QUESTION_TYPE, QUIZ_STATUS } from '@/constants/quiz'
import { changeQuestionType, emptyQuestion, emptyQuiz, fromQuizDetail, markCorrect, moveItem, parsePoints, toQuizPayload, totalPoints, validateQuizForm } from './quizForm'

function validForm() {
  const form = emptyQuiz()
  form.title = '  Kiểm tra  '
  form.questions[0].questionMarkdown = ' Thủ đô? '
  form.questions[0].options[0].optionMarkdown = 'Hà Nội'
  form.questions[0].options[1].optionMarkdown = 'Huế'
  return form
}

describe('quiz form helpers', () => {
  it('parses points with comma or dot and rejects invalid values', () => {
    expect(parsePoints('2,5')).toBe(2.5)
    expect(parsePoints('10')).toBe(10)
    expect(parsePoints('0')).toBeNull()
    expect(parsePoints('101')).toBeNull()
    expect(parsePoints('1.234')).toBeNull()
    expect(parsePoints('abc')).toBeNull()
  })

  it('builds a trimmed payload, nulling an empty description', () => {
    const payload = toQuizPayload(validForm())

    expect(payload).toMatchObject({ title: 'Kiểm tra', descriptionMarkdown: null, status: QUIZ_STATUS.draft })
    expect(payload.questions[0]).toMatchObject({ questionMarkdown: 'Thủ đô?', points: 1, questionType: QUESTION_TYPE.single })
    expect(payload.questions[0].options).toEqual([{ optionMarkdown: 'Hà Nội', isCorrect: true }, { optionMarkdown: 'Huế', isCorrect: false }])
  })

  it('omits the question tree when structure is locked by existing attempts', () => {
    expect(toQuizPayload(validForm(), { includeQuestions: false })).not.toHaveProperty('questions')
  })

  it('accepts a valid form and reports every broken rule with its question', () => {
    expect(validateQuizForm(validForm()).count).toBe(0)

    const form = validForm()
    form.title = 'a'
    form.questions[0].questionMarkdown = ' '
    form.questions[0].points = '0'
    form.questions[0].options[0].isCorrect = false
    form.questions[0].options[1].optionMarkdown = ''
    const errors = validateQuizForm(form)
    const found = errors.questions[form.questions[0].key]

    expect(errors.title).toBeTruthy()
    expect(Object.keys(found)).toEqual(expect.arrayContaining(['questionMarkdown', 'points', 'correct', `option:${form.questions[0].options[1].key}`]))
  })

  it('requires at least one question to publish and skips question checks when locked', () => {
    const form = { ...validForm(), status: QUIZ_STATUS.published, questions: [] }

    expect(validateQuizForm(form).status).toBeTruthy()
    expect(validateQuizForm({ ...form, questions: [{ ...emptyQuestion() }] }, { includeQuestions: false }).count).toBe(0)
  })

  it('requires an extra correct answer rule per question type', () => {
    const form = validForm()
    form.questions[0].questionType = QUESTION_TYPE.multiple
    form.questions[0].options[0].isCorrect = false

    expect(validateQuizForm(form).questions[form.questions[0].key].correct).toBeTruthy()
    form.questions[0].options.forEach((option) => { option.isCorrect = true })
    expect(validateQuizForm(form).count).toBe(0)
    form.questions[0].questionType = QUESTION_TYPE.single
    expect(validateQuizForm(form).questions[form.questions[0].key].correct).toBeTruthy()
  })

  it('marks correct answers exclusively for single choice and toggles for multiple choice', () => {
    const question = emptyQuestion()
    const [first, second] = question.options

    const single = markCorrect(question, second.key)
    expect(single.options.map((option) => option.isCorrect)).toEqual([false, true])

    const multiple = markCorrect(markCorrect({ ...question, questionType: QUESTION_TYPE.multiple }, second.key), first.key)
    expect(multiple.options.map((option) => option.isCorrect)).toEqual([false, true])
  })

  it('keeps only the first correct option when switching to single choice', () => {
    const question = { ...emptyQuestion(), questionType: QUESTION_TYPE.multiple }
    question.options = question.options.map((option) => ({ ...option, isCorrect: true }))

    const converted = changeQuestionType(question, QUESTION_TYPE.single)

    expect(converted.options.map((option) => option.isCorrect)).toEqual([true, false])
    expect(changeQuestionType(question, QUESTION_TYPE.multiple)).toBe(question)
  })

  it('round-trips a server detail and totals points', () => {
    const form = fromQuizDetail({
      title: 'Quiz', descriptionMarkdown: null, status: 'PUBLISHED',
      questions: [
        { questionMarkdown: 'Q1', questionType: 'SINGLE_CHOICE', points: 2, options: [{ optionMarkdown: 'A', isCorrect: true }, { optionMarkdown: 'B', isCorrect: false }] },
        { questionMarkdown: 'Q2', questionType: 'MULTIPLE_CHOICE', points: 3.5, options: [{ optionMarkdown: 'C', isCorrect: true }, { optionMarkdown: 'D', isCorrect: true }] },
      ],
    })

    expect(form.descriptionMarkdown).toBe('')
    expect(totalPoints(form)).toBe(5.5)
    expect(new Set(form.questions.flatMap((question) => [question.key, ...question.options.map((option) => option.key)])).size).toBe(6)
  })

  it('moves items without mutating and ignores out-of-range moves', () => {
    const list = ['a', 'b', 'c']

    expect(moveItem(list, 0, 2)).toEqual(['b', 'c', 'a'])
    expect(moveItem(list, 0, -1)).toBe(list)
    expect(list).toEqual(['a', 'b', 'c'])
  })
})
