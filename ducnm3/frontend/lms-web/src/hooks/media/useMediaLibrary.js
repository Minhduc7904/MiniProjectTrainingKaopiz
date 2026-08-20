import { useCallback, useEffect, useRef } from 'react'
import { useDispatch, useSelector } from 'react-redux'
import {
  fetchMediaLibrary,
  selectMediaLibraryBucket,
} from '@/features/media/mediaLibrarySlice'

export function useMediaLibrary(mediaType = '', enabled = true) {
  const dispatch = useDispatch()
  const bucket = useSelector((state) => selectMediaLibraryBucket(state, mediaType))
  const wasEnabled = useRef(false)
  const previousMediaType = useRef(mediaType)

  const load = useCallback((cursor = null) => dispatch(fetchMediaLibrary({ mediaType, cursor })), [dispatch, mediaType])

  useEffect(() => {
    if (enabled && (!wasEnabled.current || previousMediaType.current !== mediaType)) load()
    wasEnabled.current = enabled
    previousMediaType.current = mediaType
  }, [enabled, load, mediaType])

  return {
    ...bucket,
    loadMore: () => load(bucket.nextCursor),
    reload: () => load(),
  }
}
