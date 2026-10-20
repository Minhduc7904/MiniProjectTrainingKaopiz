import { useCallback, useEffect, useRef } from 'react'
import { useDispatch, useSelector } from 'react-redux'
import { AI_LIST_DEFAULT_QUERY } from '@/constants/ai'
import {
  fetchAiOptions,
  fetchAiProviders,
  removeAiProvider,
  saveAiProvider,
  setAiProvidersQuery,
} from '@/features/ai/aiSlice'

export function useAiProviders() {
  const dispatch = useDispatch()
  const list = useSelector((state) => state.ai.providers)
  const options = useSelector((state) => state.ai.options)
  const mutation = useSelector((state) => state.ai.mutation)
  const didLoad = useRef(false)

  const setQuery = useCallback((query) => dispatch(setAiProvidersQuery(query)), [dispatch])
  const load = useCallback((query) => dispatch(fetchAiProviders(query)), [dispatch])
  const reset = useCallback(() => dispatch(fetchAiProviders(AI_LIST_DEFAULT_QUERY)), [dispatch])

  // Lưu/xóa xong thì tải lại danh sách theo query hiện tại; reject (đã có toast) thì trả false để form giữ nguyên.
  const save = useCallback(
    async (input) => {
      const result = await dispatch(saveAiProvider(input))
      if (saveAiProvider.rejected.match(result)) return false
      dispatch(fetchAiProviders(list.query))
      return true
    },
    [dispatch, list.query],
  )
  const remove = useCallback(
    async (providerId) => {
      const result = await dispatch(removeAiProvider(providerId))
      if (removeAiProvider.rejected.match(result)) return false
      dispatch(fetchAiProviders(list.query))
      return true
    },
    [dispatch, list.query],
  )

  useEffect(() => {
    if (didLoad.current) return
    didLoad.current = true
    dispatch(fetchAiProviders(list.query))
    dispatch(fetchAiOptions())
  }, [dispatch, list.query])

  return {
    data: list.data,
    pagination: list.pagination,
    query: list.query,
    loading: list.loading,
    success: list.success,
    error: list.error,
    traceId: list.traceId,
    options: options.data,
    mutation,
    setQuery,
    load,
    reset,
    save,
    remove,
  }
}
