import { describe, expect, it } from 'vitest'
import { QUESTION_TYPE } from '@/constants/quiz'
import {
  answersToMap,
  countAnswered,
  formatPoints,
  formatRelativeTime,
  mapToPayload,
  scorePercent,
  summarizeResult,
  toggleOption,
  unansweredIndexes,
} from './quizAnswers'

const single = { id: 'q1', questionType: QUESTION_TYPE.single }
const multiple = { id: 'q2', questionType: QUESTION_TYPE.multiple }

describe('quiz answer helpers', () => {
  it('maps saved server answers and ignores empty ones', () => {
    expect(answersToMap([{ questionId: 'q1', optionIds: ['a'] }, { questionId: 'q2', optionIds: [] }])).toEqual({ q1: ['a'] })
    expect(answersToMap()).toEqual({})
  })

  it('selecting on a single-choice question replaces the previous option', () => {
    const first = toggleOption({}, single, 'a')
    const second = toggleOption(first, single, 'b')

    expect(second).toEqual({ q1: ['b'] })
    expect(first).toEqual({ q1: ['a'] })
  })

  it('re-selecting the chosen single-choice option keeps the same map instance', () => {
    const map = { q1: ['a'] }

    expect(toggleOption(map, single, 'a')).toBe(map)
  })

  it('toggles options on a multiple-choice question and drops the key when empty', () => {
    const one = toggleOption({}, multiple, 'a')
    const two = toggleOption(one, multiple, 'b')
    const back = toggleOption(toggleOption(two, multiple, 'a'), multiple, 'b')

    expect(two).toEqual({ q2: ['a', 'b'] })
    expect(back).toEqual({})
  })

  it('builds a payload that includes unanswered questions so stale drafts are cleared', () => {
    expect(mapToPayload([single, multiple], { q2: ['a', 'b'] })).toEqual([
      { questionId: 'q1', optionIds: [] },
      { questionId: 'q2', optionIds: ['a', 'b'] },
    ])
  })

  it('counts answered questions and lists the blank ones', () => {
    const questions = [single, multiple, { id: 'q3' }]

    expect(countAnswered(questions, { q1: ['a'] })).toBe(1)
    expect(unansweredIndexes(questions, { q1: ['a'] })).toEqual([1, 2])
  })

  it('formats points and percentages', () => {
    expect(formatPoints(5)).toBe('5')
    expect(formatPoints(5.5)).toBe('5,5')
    expect(formatPoints(null)).toBe('—')
    expect(scorePercent(5.5, 11)).toBe(50)
    expect(scorePercent(1, 0)).toBe(0)
  })

  it('describes recent times relatively', () => {
    const now = Date.parse('2026-10-20T10:00:00Z')

    expect(formatRelativeTime('2026-10-20T09:59:50Z', now)).toBe('vừa xong')
    expect(formatRelativeTime('2026-10-20T09:50:00Z', now)).toBe('10 phút trước')
    expect(formatRelativeTime('2026-10-20T07:00:00Z', now)).toBe('3 giờ trước')
    expect(formatRelativeTime(null, now)).toBe('')
  })

  it('summarises a graded result', () => {
    expect(summarizeResult({ questions: [{ correct: true, answered: true }, { correct: false, answered: true }, { correct: false, answered: false }] }))
      .toEqual({ correct: 1, incorrect: 1, unanswered: 1 })
  })
})
