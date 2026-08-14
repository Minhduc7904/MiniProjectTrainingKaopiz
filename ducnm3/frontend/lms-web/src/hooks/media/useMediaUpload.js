import { useCallback } from 'react'
import { useDispatch, useSelector } from 'react-redux'
import {
  resetMediaUpload,
  setMediaQuery,
  uploadMedia,
} from '@/features/media/mediaSlice'

export function useMediaUpload() {
  const dispatch = useDispatch()
  const upload = useSelector((state) => state.media.upload)

  const setQuery = useCallback(
    (query) => {
      dispatch(setMediaQuery(query))
    },
    [dispatch],
  )

  const submit = useCallback(
    (form) => {
      return dispatch(uploadMedia(form))
    },
    [dispatch],
  )

  const reset = useCallback(() => {
    dispatch(resetMediaUpload())
  }, [dispatch])

  return {
    data: upload.data,
    query: upload.query,
    loading: upload.loading,
    success: upload.success,
    error: upload.error,
    traceId: upload.traceId,
    location: upload.location,
    setQuery,
    submit,
    reset,
  }
}
