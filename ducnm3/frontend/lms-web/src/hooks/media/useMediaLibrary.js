import { useCallback, useEffect, useRef } from 'react'
import { useDispatch, useSelector } from 'react-redux'
import {
  fetchMediaLibrary,
  selectMediaLibraryBucket,
} from '@/features/media/mediaLibrarySlice'

export function useMediaLibrary(mediaType = '', status = '', enabled = true) {
  const dispatch = useDispatch()
  const bucket = useSelector((state) => selectMediaLibraryBucket(state, mediaType, status))
  const wasEnabled = useRef(false)
  const previousFilter = useRef({ mediaType, status })

  const load = useCallback(
    (cursor = null) => dispatch(fetchMediaLibrary({ mediaType, status, cursor })),
    [dispatch, mediaType, status],
  )

  useEffect(() => {
    const filterChanged = previousFilter.current.mediaType !== mediaType ||
      previousFilter.current.status !== status
    if (enabled && (!wasEnabled.current || filterChanged)) load()
    wasEnabled.current = enabled
    previousFilter.current = { mediaType, status }
  }, [enabled, load, mediaType, status])

  return {
    ...bucket,
    loadMore: () => load(bucket.nextCursor),
    reload: () => load(),
  }
}
