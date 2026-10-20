import { useCallback, useEffect, useState } from 'react'
import { fetchQuizAttemptRequest, fetchQuizAttemptsRequest, fetchStudentQuizRequest, fetchStudentQuizzesRequest } from '@/api/studentQuizApi'
import { toApiError } from '@/api/toApiError'

const IDLE = { data: null, loading: false, error: null }

/** Tải một tài nguyên quiz của học viên, tự hủy request cũ và cho phép tải lại bằng `reload`. */
function useStudentQuizResource(load, deps) {
  const [state, setState] = useState(IDLE)
  const [version, setVersion] = useState(0)

  useEffect(() => {
    if (deps.some((value) => !value)) {
      setState(IDLE)
      return undefined
    }
    const controller = new AbortController()
    setState((current) => ({ ...current, loading: true, error: null }))
    load({ signal: controller.signal })
      .then(({ data }) => setState({ data, loading: false, error: null }))
      .catch((error) => { if (!controller.signal.aborted) setState({ data: null, loading: false, error: toApiError(error) }) })
    return () => controller.abort()
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [...deps, version])

  const reload = useCallback(() => setVersion((current) => current + 1), [])
  return { ...state, reload }
}

export function useStudentLessonQuizzes(courseId, lessonId) {
  const { data, ...rest } = useStudentQuizResource((options) => fetchStudentQuizzesRequest(courseId, lessonId, options), [courseId, lessonId])
  return { quizzes: data ?? [], ...rest }
}

export function useStudentQuizDetail(courseId, lessonId, quizId) {
  const { data, ...rest } = useStudentQuizResource((options) => fetchStudentQuizRequest(courseId, lessonId, quizId, options), [courseId, lessonId, quizId])
  return { quiz: data, ...rest }
}

export function useQuizAttemptHistory(courseId, lessonId, quizId) {
  const { data, ...rest } = useStudentQuizResource((options) => fetchQuizAttemptsRequest(courseId, lessonId, quizId, options), [courseId, lessonId, quizId])
  return { attempts: data ?? [], ...rest }
}

export function useQuizAttemptDetail(courseId, lessonId, quizId, attemptId) {
  const { data, ...rest } = useStudentQuizResource((options) => fetchQuizAttemptRequest(courseId, lessonId, quizId, attemptId, options), [courseId, lessonId, quizId, attemptId])
  return { detail: data, ...rest }
}
