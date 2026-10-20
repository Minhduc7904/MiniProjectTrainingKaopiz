// @vitest-environment jsdom
import { cleanup, fireEvent, render, screen } from '@testing-library/react'
import { afterEach, describe, expect, it, vi } from 'vitest'
import { QUESTION_TYPE } from '@/constants/quiz'
import { QuestionCard } from './QuestionCard'

afterEach(cleanup)

const options = [
  { id: 'a', optionHtml: '<p>Hà Nội</p>' },
  { id: 'b', optionHtml: '<p>Huế <strong>xưa</strong></p>' },
]
const single = { id: 'q1', questionType: QUESTION_TYPE.single, points: 2, questionHtml: '<p>Thủ đô?</p>', options }
const multiple = { ...single, id: 'q2', questionType: QUESTION_TYPE.multiple, points: 3.5 }

describe('QuestionCard', () => {
  it('renders a single-choice question as a radio group with Markdown HTML content', () => {
    const onSelect = vi.fn()
    render(<QuestionCard question={single} index={0} total={3} selectedIds={['b']} onSelect={onSelect} />)

    expect(screen.getAllByRole('radio')).toHaveLength(2)
    expect(screen.getByRole('radiogroup', { name: 'Đáp án câu 1' })).not.toBeNull()
    expect(screen.getByText('Thủ đô?')).not.toBeNull()
    expect(screen.getByText('xưa').tagName).toBe('STRONG')
    expect(screen.getByRole('radio', { name: 'Đáp án B' }).checked).toBe(true)
    expect(screen.getByText('Chọn 1 đáp án')).not.toBeNull()
    expect(screen.getByText('2 điểm')).not.toBeNull()

    fireEvent.click(screen.getByRole('radio', { name: 'Đáp án A' }))
    expect(onSelect).toHaveBeenCalledWith('a')
  })

  it('renders a multiple-choice question as checkboxes with decimal points', () => {
    render(<QuestionCard question={multiple} index={1} total={3} selectedIds={['a', 'b']} onSelect={() => {}} />)

    expect(screen.getAllByRole('checkbox')).toHaveLength(2)
    expect(screen.getByRole('group', { name: 'Đáp án câu 2' })).not.toBeNull()
    expect(screen.getByText('3,5 điểm')).not.toBeNull()
    expect(screen.getByText('Chọn nhiều đáp án')).not.toBeNull()
  })

  it('lets the student flag a question for review', () => {
    const onToggleFlag = vi.fn()
    render(<QuestionCard question={single} index={0} total={1} onSelect={() => {}} flagged onToggleFlag={onToggleFlag} />)

    fireEvent.click(screen.getByRole('button', { name: 'Đã đánh dấu xem lại' }))
    expect(onToggleFlag).toHaveBeenCalled()
  })

  it('marks review states with text, not colour alone, and locks the inputs', () => {
    const grade = { correct: false, answered: true, pointsEarned: 0, pointsPossible: 2, correctOptionIds: ['a'] }
    render(<QuestionCard question={single} index={0} total={1} selectedIds={['b']} grade={grade} />)

    expect(screen.getByText('Bạn chọn · Sai')).not.toBeNull()
    expect(screen.getByText('Đáp án đúng')).not.toBeNull()
    expect(screen.getByText('Chưa đúng')).not.toBeNull()
    expect(screen.getByText('0/2 điểm')).not.toBeNull()
    expect(screen.getAllByRole('radio').every((input) => input.disabled)).toBe(true)
  })
})
