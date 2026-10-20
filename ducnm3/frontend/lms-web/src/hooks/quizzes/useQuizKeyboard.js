import { useEffect, useRef } from 'react'

const EDITABLE = new Set(['INPUT', 'TEXTAREA', 'SELECT'])

function isTypingTarget(target) {
  return target instanceof HTMLElement && ((EDITABLE.has(target.tagName) && target.type !== 'radio' && target.type !== 'checkbox') || target.isContentEditable)
}

/** Phím tắt khi làm bài: ←/→ đổi câu, 1–9 hoặc A–F chọn đáp án, Ctrl/⌘ + Enter mở hộp nộp bài. */
export function useQuizKeyboard({ enabled, onPrevious, onNext, onSelectIndex, onSubmit }) {
  const handlersRef = useRef({})
  handlersRef.current = { onPrevious, onNext, onSelectIndex, onSubmit }

  useEffect(() => {
    if (!enabled) return undefined
    const onKeyDown = (event) => {
      if (event.defaultPrevented || event.altKey || isTypingTarget(event.target)) return
      const handlers = handlersRef.current
      if (event.key === 'Enter' && (event.ctrlKey || event.metaKey)) {
        event.preventDefault()
        handlers.onSubmit?.()
        return
      }
      if (event.ctrlKey || event.metaKey) return
      if (event.key === 'ArrowLeft') { event.preventDefault(); handlers.onPrevious?.(); return }
      if (event.key === 'ArrowRight') { event.preventDefault(); handlers.onNext?.(); return }
      const digitIndex = /^[1-9]$/.test(event.key) ? Number(event.key) - 1 : -1
      const letterIndex = event.key.length === 1 ? 'ABCDEF'.indexOf(event.key.toUpperCase()) : -1
      const index = digitIndex >= 0 ? digitIndex : letterIndex
      if (index >= 0) {
        event.preventDefault()
        handlers.onSelectIndex?.(index)
      }
    }
    document.addEventListener('keydown', onKeyDown)
    return () => document.removeEventListener('keydown', onKeyDown)
  }, [enabled])
}
