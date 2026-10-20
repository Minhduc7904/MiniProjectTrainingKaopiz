// @vitest-environment jsdom
import { cleanup, fireEvent, render } from '@testing-library/react'
import { afterEach, describe, expect, it, vi } from 'vitest'
import { useQuizKeyboard } from './useQuizKeyboard'

afterEach(cleanup)

function Harness({ enabled = true, handlers }) {
  useQuizKeyboard({ enabled, ...handlers })
  return <input aria-label="ghi chú" />
}

const handlers = () => ({ onPrevious: vi.fn(), onNext: vi.fn(), onSelectIndex: vi.fn(), onSubmit: vi.fn() })

describe('useQuizKeyboard', () => {
  it('maps arrows, digits, letters and Ctrl/Cmd+Enter to quiz actions', () => {
    const h = handlers()
    render(<Harness handlers={h} />)

    fireEvent.keyDown(document, { key: 'ArrowRight' })
    fireEvent.keyDown(document, { key: 'ArrowLeft' })
    fireEvent.keyDown(document, { key: '2' })
    fireEvent.keyDown(document, { key: 'c' })
    fireEvent.keyDown(document, { key: 'Enter', ctrlKey: true })
    fireEvent.keyDown(document, { key: 'Enter', metaKey: true })

    expect(h.onNext).toHaveBeenCalledTimes(1)
    expect(h.onPrevious).toHaveBeenCalledTimes(1)
    expect(h.onSelectIndex.mock.calls).toEqual([[1], [2]])
    expect(h.onSubmit).toHaveBeenCalledTimes(2)
  })

  it('ignores shortcuts while typing, with modifiers, for unsupported letters and when disabled', () => {
    const h = handlers()
    const { getByLabelText, rerender } = render(<Harness handlers={h} />)

    fireEvent.keyDown(getByLabelText('ghi chú'), { key: '1' })
    fireEvent.keyDown(document, { key: 'g' })
    fireEvent.keyDown(document, { key: 'a', ctrlKey: true })
    rerender(<Harness enabled={false} handlers={h} />)
    fireEvent.keyDown(document, { key: 'ArrowRight' })

    expect(h.onSelectIndex).not.toHaveBeenCalled()
    expect(h.onNext).not.toHaveBeenCalled()
  })
})
