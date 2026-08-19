import { useCallback, useEffect, useRef } from 'react'
import { useDispatch, useSelector } from 'react-redux'
import { GET_COURSES_DEFAULT_QUERY } from '@/constants/inputs/getCourses'
import { fetchCoursesList, setCoursesQuery } from '@/features/courses/coursesSlice'
export function useCoursesList() {
  const dispatch = useDispatch(); const list = useSelector((state) => state.courses.list); const didLoad = useRef(false)
  const load = useCallback((query) => dispatch(fetchCoursesList(query)), [dispatch])
  useEffect(() => { if (!didLoad.current) { didLoad.current = true; dispatch(fetchCoursesList(list.query)) } }, [dispatch, list.query])
  return { ...list, setQuery: (query) => dispatch(setCoursesQuery(query)), load, reset: () => dispatch(fetchCoursesList(GET_COURSES_DEFAULT_QUERY)) }
}
