import { useCallback, useEffect, useRef } from 'react'
import { useDispatch, useSelector } from 'react-redux'
import { GET_STUDENTS_DEFAULT_QUERY } from '@/constants/inputs/getStudents'
import {
  fetchStudentsList,
  setStudentsQuery,
} from '@/features/students/studentsSlice'

export function useStudentsList() {
  const dispatch = useDispatch()
  const list = useSelector((state) => state.students.list)
  const didLoad = useRef(false)

  const setQuery = useCallback(
    (query) => {
      dispatch(setStudentsQuery(query))
    },
    [dispatch],
  )

  const load = useCallback(
    (query) => {
      dispatch(fetchStudentsList(query))
    },
    [dispatch],
  )

  const reset = useCallback(() => {
    dispatch(fetchStudentsList(GET_STUDENTS_DEFAULT_QUERY))
  }, [dispatch])

  useEffect(() => {
    if (didLoad.current) {
      return
    }

    didLoad.current = true
    dispatch(fetchStudentsList(list.query))
  }, [dispatch, list.query])

  return {
    data: list.data,
    pagination: list.pagination,
    query: list.query,
    loading: list.loading,
    success: list.success,
    error: list.error,
    traceId: list.traceId,
    setQuery,
    load,
    reset,
  }
}
