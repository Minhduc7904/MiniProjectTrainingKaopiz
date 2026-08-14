import { useEffect, useRef, useState } from 'react'
import { TOAST_PHASE } from '@/constants/toast'

export function useToastProgress({ id, phase, durationMs, paused, onElapsed }) {
  const [progress, setProgress] = useState(0)
  const remainingRef = useRef(durationMs)
  const lastFrameRef = useRef(null)
  const elapsedRef = useRef(false)

  useEffect(() => {
    remainingRef.current = durationMs
    lastFrameRef.current = null
    elapsedRef.current = false
    setProgress(0)
  }, [id, phase, durationMs])

  useEffect(() => {
    if (paused) {
      lastFrameRef.current = null
      return undefined
    }

    let frameId = 0

    const tick = (now) => {
      if (lastFrameRef.current == null) {
        lastFrameRef.current = now
      }

      const delta = now - lastFrameRef.current
      lastFrameRef.current = now
      remainingRef.current = Math.max(0, remainingRef.current - delta)
      const ratio =
        durationMs <= 0 ? 1 : 1 - remainingRef.current / durationMs
      setProgress(ratio)

      if (remainingRef.current <= 0) {
        if (!elapsedRef.current) {
          elapsedRef.current = true
          onElapsed()
        }
        return
      }

      frameId = requestAnimationFrame(tick)
    }

    frameId = requestAnimationFrame(tick)

    return () => {
      cancelAnimationFrame(frameId)
    }
  }, [paused, durationMs, id, phase, onElapsed])

  return {
    progress,
    isCountdownDone: progress >= 1,
    autoHide: phase !== TOAST_PHASE.pending,
  }
}
