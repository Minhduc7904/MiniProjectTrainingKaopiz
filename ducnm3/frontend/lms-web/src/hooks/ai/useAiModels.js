import { useCallback, useEffect, useRef } from 'react'
import { useDispatch, useSelector } from 'react-redux'
import { AI_CHOICES_QUERY, AI_LIST_DEFAULT_QUERY } from '@/constants/ai'
import {
  fetchAiModels,
  fetchAiProviderChoices,
  removeAiModel,
  saveAiModel,
  setAiModelsQuery,
} from '@/features/ai/aiSlice'

export function useAiModels() {
  const dispatch = useDispatch()
  const list = useSelector((state) => state.ai.models)
  const providerChoices = useSelector((state) => state.ai.providerChoices)
  const mutation = useSelector((state) => state.ai.mutation)
  const didLoad = useRef(false)

  const setQuery = useCallback((query) => dispatch(setAiModelsQuery(query)), [dispatch])
  const load = useCallback((query) => dispatch(fetchAiModels(query)), [dispatch])
  const reset = useCallback(() => dispatch(fetchAiModels(AI_LIST_DEFAULT_QUERY)), [dispatch])

  const save = useCallback(
    async (input) => {
      const result = await dispatch(saveAiModel(input))
      if (saveAiModel.rejected.match(result)) return false
      dispatch(fetchAiModels(list.query))
      return true
    },
    [dispatch, list.query],
  )
  const remove = useCallback(
    async (modelId) => {
      const result = await dispatch(removeAiModel(modelId))
      if (removeAiModel.rejected.match(result)) return false
      dispatch(fetchAiModels(list.query))
      return true
    },
    [dispatch, list.query],
  )

  useEffect(() => {
    if (didLoad.current) return
    didLoad.current = true
    dispatch(fetchAiModels(list.query))
    dispatch(fetchAiProviderChoices(AI_CHOICES_QUERY))
  }, [dispatch, list.query])

  return {
    data: list.data,
    pagination: list.pagination,
    query: list.query,
    loading: list.loading,
    success: list.success,
    error: list.error,
    traceId: list.traceId,
    providers: providerChoices.data,
    mutation,
    setQuery,
    load,
    reset,
    save,
    remove,
  }
}
