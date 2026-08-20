import { useCallback, useEffect } from 'react'
import { useDispatch, useSelector } from 'react-redux'
import { getMediaThumbnailRequest, retryMediaThumbnailRequest } from '@/api/mediaApi'
import { mediaThumbnailActions } from '@/features/media/mediaThumbnailSlice'

const ACTIVE_THUMBNAIL_STATUSES = new Set(['QUEUED', 'PROCESSING'])

export function shouldPollThumbnail(thumbnail) {
  return Boolean(thumbnail?.thumbnailStatusUrl) && ACTIVE_THUMBNAIL_STATUSES.has(thumbnail.status)
}

export function isImageMedia(media) {
  return media?.mediaType === 'IMAGE' && String(media.contentType ?? '').startsWith('image/')
}

function toInitialThumbnail(media) {
  return {
    status: media?.thumbnailStatus ?? null,
    thumbnailStatusUrl: media?.thumbnailStatusUrl ?? null,
    thumbnailContentUrl: null,
    thumbnailMediaId: media?.thumbnailMediaId ?? null,
    lastError: null,
  }
}

export function useMediaThumbnail(media) {
  const dispatch = useDispatch()
  const state = useSelector((rootState) => rootState.mediaThumbnail)
  const thumbnail = state.data ?? toInitialThumbnail(media)
  const thumbnailStatus = thumbnail.status
  const thumbnailStatusUrl = thumbnail.thumbnailStatusUrl

  const refresh = useCallback(async (signal) => {
    if (!media?.thumbnailStatusUrl) return null
    dispatch(mediaThumbnailActions.pollStarted({ mediaId: media.id }))
    try {
      const response = await getMediaThumbnailRequest(media.thumbnailStatusUrl, { signal })
      dispatch(mediaThumbnailActions.pollSucceeded({ mediaId: media.id, thumbnail: response.data }))
      return response.data
    } catch (requestError) {
      if (requestError?.name !== 'CanceledError') dispatch(mediaThumbnailActions.pollFailed(requestError))
      return null
    }
  }, [dispatch, media?.id, media?.thumbnailStatusUrl])

  useEffect(() => {
    dispatch(mediaThumbnailActions.initialized({ mediaId: media?.id, thumbnail: toInitialThumbnail(media) }))
  }, [dispatch, media])

  useEffect(() => {
    if (!thumbnailStatusUrl || !ACTIVE_THUMBNAIL_STATUSES.has(thumbnailStatus)) return undefined
    const controller = new AbortController()
    const poll = () => refresh(controller.signal)
    poll()
    const interval = window.setInterval(poll, 1500)
    return () => {
      controller.abort()
      window.clearInterval(interval)
    }
  }, [refresh, thumbnailStatus, thumbnailStatusUrl])

  const retry = useCallback(async () => {
    if (!media?.id) return false
    dispatch(mediaThumbnailActions.retryStarted())
    try {
      const response = await retryMediaThumbnailRequest(media.id)
      dispatch(mediaThumbnailActions.retrySucceeded({
        mediaId: media.id,
        thumbnail: { ...response.data, thumbnailStatusUrl: media.thumbnailStatusUrl },
      }))
      return true
    } catch (requestError) {
      dispatch(mediaThumbnailActions.retryFailed(requestError))
      return false
    } finally {
    }
  }, [dispatch, media?.id, media?.thumbnailStatusUrl])

  return { thumbnail, error: state.error, retrying: state.retrying, retry }
}
