import { useCallback, useEffect, useRef, useState } from 'react'
import { saveQuizAnswersRequest, startQuizAttemptRequest, submitQuizAttemptRequest } from '@/api/studentQuizApi'
import { toApiError } from '@/api/toApiError'
import { QUIZ_AUTOSAVE_DELAY_MS, QUIZ_RETRY_DELAYS_MS, QUIZ_SAVE_STATE } from '@/constants/quiz'
import { answersToMap, mapToPayload, toggleOption } from '@/utils/quizAnswers'

const START_IDLE = { status: 'idle', error: null }
const HTTP_CONFLICT = 409

function isDirty(session) {
  return session.version !== session.savedVersion
}

function payloadOf(session) {
  return mapToPayload(session.questions, session.answers)
}

/**
 * Vòng đời một lần làm bài: bắt đầu hoặc tiếp tục bài đang làm dở, tự lưu nháp (debounce, mỗi lần chỉ một request,
 * thử lại khi lỗi mạng) và nộp bài kèm đáp án cuối. Mỗi lần bắt đầu tạo một "session" riêng nên phản hồi muộn của
 * bài làm cũ không bao giờ ghi đè trạng thái của bài làm mới.
 */
export function useQuizAttempt({ courseId, lessonId, quiz }) {
  const quizId = quiz?.id
  const [start, setStart] = useState(START_IDLE)
  const [attempt, setAttempt] = useState(null)
  const [resumed, setResumed] = useState(false)
  const [answers, setAnswers] = useState({})
  const [saveState, setSaveState] = useState(QUIZ_SAVE_STATE.saved)
  const [saveError, setSaveError] = useState(null)
  const [lastSavedAt, setLastSavedAt] = useState(null)
  const [submit, setSubmit] = useState({ submitting: false, error: null })

  const questionsRef = useRef(quiz?.questions ?? [])
  const sessionRef = useRef(null)
  const saveStateRef = useRef(QUIZ_SAVE_STATE.saved)
  const timerRef = useRef(null)
  const retryRef = useRef(0)

  questionsRef.current = quiz?.questions ?? []

  const updateSaveState = useCallback((session, next) => {
    if (sessionRef.current !== session) return
    saveStateRef.current = next
    setSaveState(next)
  }, [])

  const persist = useCallback((session) => {
    if (session.inflight) return session.inflight
    if (session.finished) return Promise.resolve(true)

    const run = (async () => {
      while (isDirty(session) && !session.finished) {
        const version = session.version
        updateSaveState(session, QUIZ_SAVE_STATE.saving)
        try {
          await saveQuizAnswersRequest(session.courseId, session.lessonId, session.quizId, session.attemptId, payloadOf(session))
          session.savedVersion = version
          if (sessionRef.current === session) {
            retryRef.current = 0
            setSaveError(null)
            setLastSavedAt(new Date().toISOString())
          }
        } catch (error) {
          const apiError = toApiError(error)
          if (sessionRef.current === session) {
            setSaveError(apiError)
            updateSaveState(session, QUIZ_SAVE_STATE.error)
            if (apiError.httpStatus !== HTTP_CONFLICT) {
              const delay = QUIZ_RETRY_DELAYS_MS[Math.min(retryRef.current, QUIZ_RETRY_DELAYS_MS.length - 1)]
              retryRef.current += 1
              timerRef.current = setTimeout(() => void persist(session), delay)
            }
          }
          return false
        }
      }
      if (!isDirty(session)) updateSaveState(session, QUIZ_SAVE_STATE.saved)
      return true
    })().finally(() => { session.inflight = null })

    session.inflight = run
    return run
  }, [updateSaveState])

  const flush = useCallback(() => {
    clearTimeout(timerRef.current)
    return sessionRef.current ? persist(sessionRef.current) : Promise.resolve(true)
  }, [persist])

  useEffect(() => {
    if (!quizId) return undefined
    const controller = new AbortController()
    sessionRef.current = null
    saveStateRef.current = QUIZ_SAVE_STATE.saved
    retryRef.current = 0
    setStart({ status: 'starting', error: null })
    setAnswers({})
    setSaveState(QUIZ_SAVE_STATE.saved)
    setSaveError(null)
    setSubmit({ submitting: false, error: null })

    startQuizAttemptRequest(courseId, lessonId, quizId, { signal: controller.signal })
      .then(({ data, created }) => {
        const restored = answersToMap(data.answers)
        sessionRef.current = {
          courseId, lessonId, quizId, questions: questionsRef.current, attemptId: data.id,
          answers: restored, version: 0, savedVersion: 0, inflight: null, finished: false,
        }
        setAttempt(data)
        setAnswers(restored)
        setResumed(!created)
        setStart({ status: 'ready', error: null })
      })
      .catch((error) => { if (!controller.signal.aborted) setStart({ status: 'error', error: toApiError(error) }) })

    return () => {
      controller.abort()
      clearTimeout(timerRef.current)
      const session = sessionRef.current
      sessionRef.current = null
      if (session && isDirty(session) && !session.finished) void persist(session)
    }
  }, [courseId, lessonId, quizId, persist])

  useEffect(() => {
    const flushWhenHidden = () => { if (document.visibilityState === 'hidden') void flush() }
    const flushOnLeave = () => { void flush() }
    const warnBeforeUnload = (event) => {
      if (saveStateRef.current === QUIZ_SAVE_STATE.saved) return
      event.preventDefault()
      event.returnValue = ''
    }
    document.addEventListener('visibilitychange', flushWhenHidden)
    window.addEventListener('pagehide', flushOnLeave)
    window.addEventListener('beforeunload', warnBeforeUnload)
    return () => {
      document.removeEventListener('visibilitychange', flushWhenHidden)
      window.removeEventListener('pagehide', flushOnLeave)
      window.removeEventListener('beforeunload', warnBeforeUnload)
    }
  }, [flush])

  const select = useCallback((question, optionId) => {
    const session = sessionRef.current
    if (!session || session.finished) return
    const next = toggleOption(session.answers, question, optionId)
    if (next === session.answers) return
    session.answers = next
    session.version += 1
    setAnswers(next)
    updateSaveState(session, QUIZ_SAVE_STATE.dirty)
    clearTimeout(timerRef.current)
    timerRef.current = setTimeout(() => void persist(session), QUIZ_AUTOSAVE_DELAY_MS)
  }, [persist, updateSaveState])

  const retrySave = useCallback(() => {
    retryRef.current = 0
    return flush()
  }, [flush])

  const submitAttempt = useCallback(async () => {
    const session = sessionRef.current
    if (!session || session.finished) return null
    clearTimeout(timerRef.current)
    setSubmit({ submitting: true, error: null })
    try {
      await session.inflight
      const { data } = await submitQuizAttemptRequest(session.courseId, session.lessonId, session.quizId, session.attemptId, payloadOf(session))
      session.finished = true
      session.savedVersion = session.version
      updateSaveState(session, QUIZ_SAVE_STATE.saved)
      setSubmit({ submitting: false, error: null })
      return data
    } catch (error) {
      const apiError = error?.code ? error : toApiError(error)
      setSubmit({ submitting: false, error: apiError })
      throw apiError
    }
  }, [updateSaveState])

  return { start, attempt, resumed, answers, saveState, saveError, lastSavedAt, submit, select, retrySave, flush, submitAttempt }
}
