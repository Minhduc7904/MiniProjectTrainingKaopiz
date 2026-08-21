import { useCallback, useEffect, useRef } from 'react'
import { useDispatch, useSelector } from 'react-redux'
import { GET_MEDIA_JOBS_DEFAULT_QUERY } from '@/constants/inputs/getMediaJobs'
import { fetchMediaJobs, setMediaJobsQuery } from '@/features/media/mediaJobsSlice'

export function useMediaJobsList() {
  const dispatch = useDispatch()
  const list = useSelector((state) => state.mediaJobs.list)
  const didLoad = useRef(false)
  const setQuery = useCallback((query) => dispatch(setMediaJobsQuery(query)), [dispatch])
  const load = useCallback((query) => dispatch(fetchMediaJobs(query)), [dispatch])
  const reset = useCallback(() => dispatch(fetchMediaJobs(GET_MEDIA_JOBS_DEFAULT_QUERY)), [dispatch])

  useEffect(() => {
    if (didLoad.current) return
    didLoad.current = true
    dispatch(fetchMediaJobs(list.query))
  }, [dispatch, list.query])

  return { ...list, setQuery, load, reset }
}
