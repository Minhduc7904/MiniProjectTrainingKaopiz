// @vitest-environment jsdom
import { cleanup, fireEvent, render, screen } from '@testing-library/react'
import { afterEach, describe, expect, it, vi } from 'vitest'
import { QuizSubmitDialog } from './QuizSubmitDialog'

afterEach(cleanup)

const baseProps = { open: true, total: 5, answered: 3, blankIndexes: [1, 4], submitting: false, error: null, onGo: () => {}, onCancel: () => {}, onConfirm: () => {} }

describe('QuizSubmitDialog', () => {
  it('shows progress and lists blank questions as jump links', () => {
    const onGo = vi.fn()
    render(<QuizSubmitDialog {...baseProps} onGo={onGo} />)

    expect(screen.getByText('3/5')).not.toBeNull()
    expect(screen.getByText('Còn 2 câu chưa trả lời')).not.toBeNull()
    fireEvent.click(screen.getByRole('button', { name: 'Câu 5' }))
    expect(onGo).toHaveBeenCalledWith(4)
  })

  it('omits the blank list when everything is answered and exposes submit errors', () => {
    const onConfirm = vi.fn()
    render(<QuizSubmitDialog {...baseProps} answered={5} blankIndexes={[]} error={{ message: 'Mất kết nối' }} onConfirm={onConfirm} />)

    expect(screen.queryByText(/chưa trả lời/)).toBeNull()
    expect(screen.getByRole('alert').textContent).toBe('Mất kết nối')
    fireEvent.click(screen.getByRole('button', { name: 'Nộp bài' }))
    expect(onConfirm).toHaveBeenCalled()
  })

  it('disables both actions while submitting', () => {
    render(<QuizSubmitDialog {...baseProps} submitting />)

    expect(screen.getByRole('button', { name: 'Đang nộp…' }).disabled).toBe(true)
    expect(screen.getByRole('button', { name: 'Tiếp tục làm' }).disabled).toBe(true)
  })
})
