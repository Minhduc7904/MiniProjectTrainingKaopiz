import { useCallback, useEffect, useRef } from 'react'
import { useDispatch, useSelector } from 'react-redux'
import { appendMedia } from '@/features/media/mediaLibrarySlice'
import { readActor } from '@/auth/actorStorage'
import {
  completeDirectUploadRequest,
  createUploadIntentRequest,
  uploadToSignedUrl,
} from '@/api/directMediaUploadApi'
import { toApiError } from '@/api/toApiError'
import { sanitizeApiError } from '@/api/sanitizeApiError'
import { API_ERROR_CODES } from '@/constants/apiErrorCodes'
import { createDirectUploadRuntime } from '@/hooks/media/directUploadRuntime'
import { executeDirectUploadWorkflow } from '@/hooks/media/directUploadWorkflow'
import {
  DIRECT_UPLOAD_PHASES,
  directUploadActions,
  normalizeProgress,
  validateDirectUploadFile,
} from '@/features/media/directUploadSlice'

function hashFile(file, onProgress) {
  const worker = new Worker(
    new URL('../../workers/mediaChecksum.worker.js', import.meta.url),
    { type: 'module' },
  )
  let rejectHash
  const promise = new Promise((resolve, reject) => {
    rejectHash = reject
    worker.onmessage = (event) => {
      if (event.data?.type === 'progress') {
        onProgress(normalizeProgress(event.data.loaded, event.data.total))
      } else if (event.data?.type === 'complete') {
        resolve(event.data.checksum)
      } else if (event.data?.type === 'error') {
        reject(event.data.error)
      }
    }
    worker.onerror = () => reject({
      code: API_ERROR_CODES.checksumFailed,
      message: 'Web Worker tính checksum bị lỗi.',
      httpStatus: null,
      traceId: null,
    })
    worker.postMessage({ type: 'hash', file })
  }).finally(() => {
    worker.terminate()
  })
  promise.abort = () => {
    worker.postMessage({ type: 'cancel' })
    worker.terminate()
    rejectHash({
      code: API_ERROR_CODES.uploadCanceled,
      message: 'Tính checksum đã bị hủy.',
      httpStatus: null,
      traceId: null,
    })
  }
  return promise
}

export function useDirectMediaUpload() {
  const dispatch = useDispatch()
  const state = useSelector((rootState) => rootState.directUpload)
  const runtimeRef = useRef(null)
  const runRef = useRef(0)

  const abortRuntime = useCallback(() => {
    runRef.current += 1
    runtimeRef.current?.abort()
    runtimeRef.current = null
  }, [])

  const reset = useCallback(() => {
    abortRuntime()
    dispatch(directUploadActions.reset())
  }, [abortRuntime, dispatch])

  useEffect(() => () => {
    abortRuntime()
    dispatch(directUploadActions.reset())
  }, [abortRuntime, dispatch])

  const setQuery = useCallback(
    (query) => dispatch(directUploadActions.setQuery(query)),
    [dispatch],
  )

  const submit = useCallback(async (file) => {
    abortRuntime()
    const runId = runRef.current
    const validationError = validateDirectUploadFile(file)
    if (validationError) {
      dispatch(directUploadActions.failed(validationError))
      return false
    }
    const runtime = createDirectUploadRuntime()
    runtimeRef.current = runtime

    const persistedActor = readActor()
    const actor = {
      uploadedBy: persistedActor.id,
      uploadedByType: persistedActor.type,
    }
    const result = await executeDirectUploadWorkflow({
      file,
      actor,
      runtime,
      isCurrent: () => runRef.current === runId,
      operations: {
        hashFile,
        createIntent: createUploadIntentRequest,
        uploadSigned: uploadToSignedUrl,
        complete: completeDirectUploadRequest,
      },
      events: {
        checksumStarted: () => dispatch(directUploadActions.checksumStarted()),
        checksumProgress: (progress) => dispatch(directUploadActions.checksumProgress(progress)),
        checksumCompleted: (checksum) => dispatch(directUploadActions.checksumCompleted(checksum)),
        preparingStarted: () => dispatch(directUploadActions.preparingStarted()),
        uploadStarted: (mediaId, traceId) => dispatch(directUploadActions.uploadStarted({ mediaId, traceId })),
        uploadProgress: (progress) => dispatch(directUploadActions.uploadProgress(progress)),
        finalizeStarted: () => dispatch(directUploadActions.finalizeStarted()),
        completed: (data, traceId) => {
          dispatch(directUploadActions.completed({ data, traceId }))
          dispatch(appendMedia(data))
        },
        failed: (error) => dispatch(directUploadActions.failed(error)),
      },
      sanitizeError: (error) => sanitizeApiError(
        error?.code ? error : toApiError(error),
      ),
    })
    if (runtimeRef.current === runtime) runtimeRef.current = null
    return result
  }, [abortRuntime, dispatch])

  return {
    ...state,
    loading: [
      DIRECT_UPLOAD_PHASES.checksum,
      DIRECT_UPLOAD_PHASES.preparing,
      DIRECT_UPLOAD_PHASES.upload,
      DIRECT_UPLOAD_PHASES.finalize,
    ].includes(state.phase),
    setQuery,
    submit,
    reset,
  }
}
