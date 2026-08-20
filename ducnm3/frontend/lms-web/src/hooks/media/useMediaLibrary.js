import { useCallback, useEffect } from 'react'
import { useDispatch, useSelector } from 'react-redux'
import {
  fetchMediaLibrary,
  selectMediaLibraryBucket,
} from '@/features/media/mediaLibrarySlice'

export function useMediaLibrary(mediaType = '', enabled = true) {
  const dispatch = useDispatch()
  const bucket = useSelector((state) => selectMediaLibraryBucket(state, mediaType))

  const load = useCallback((cursor = null) => dispatch(fetchMediaLibrary({ mediaType, cursor })), [dispatch, mediaType])

  useEffect(() => {
    if (enabled && !bucket.loaded && !bucket.loading) load()
  }, [bucket.loaded, bucket.loading, enabled, load])

  return {
    ...bucket,
    loadMore: () => load(bucket.nextCursor),
    reload: () => load(),
  }
}
