import { useCallback, useEffect } from 'react'
import { useDispatch, useSelector } from 'react-redux'
import { GET_MEDIA_LIBRARY_DEFAULT_QUERY } from '@/constants/inputs/getMediaLibrary'
import {
  fetchMediaLibrary,
  selectMediaLibraryBucket,
  selectMediaLibraryExplorer,
  setMediaLibraryExplorerQuery,
  setMediaLibraryExplorerSelectedId,
} from '@/features/media/mediaLibrarySlice'

export function useMediaLibraryExplorer() {
  const dispatch = useDispatch()
  const explorer = useSelector(selectMediaLibraryExplorer)
  const library = useSelector((state) => selectMediaLibraryBucket(
    state,
    explorer.query.mediaType,
    explorer.query.status,
  ))

  const load = useCallback(
    (cursor = null) => dispatch(fetchMediaLibrary({ ...explorer.query, cursor })),
    [dispatch, explorer.query],
  )

  useEffect(() => {
    load()
  }, [load])

  const setQuery = useCallback((patch) => {
    dispatch(setMediaLibraryExplorerQuery({ ...explorer.query, ...patch }))
  }, [dispatch, explorer.query])

  const select = useCallback(
    (mediaId) => dispatch(setMediaLibraryExplorerSelectedId(mediaId)),
    [dispatch],
  )

  return {
    ...library,
    query: explorer.query,
    selectedId: explorer.selectedId,
    setQuery,
    select,
    reset: () => dispatch(setMediaLibraryExplorerQuery(GET_MEDIA_LIBRARY_DEFAULT_QUERY)),
    loadMore: () => load(library.nextCursor),
    reload: () => load(),
  }
}
