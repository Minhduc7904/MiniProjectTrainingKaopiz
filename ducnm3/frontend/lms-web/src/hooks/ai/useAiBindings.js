import { useCallback, useEffect, useRef } from 'react'
import { useDispatch, useSelector } from 'react-redux'
import { AI_CHOICES_QUERY, AI_STATUS } from '@/constants/ai'
import {
  fetchAiBindings,
  fetchAiModelChoices,
  fetchAiOptions,
  removeAiBinding,
  saveAiBinding,
} from '@/features/ai/aiSlice'

export function useAiBindings() {
  const dispatch = useDispatch()
  const list = useSelector((state) => state.ai.bindings)
  const options = useSelector((state) => state.ai.options)
  const modelChoices = useSelector((state) => state.ai.modelChoices)
  const mutation = useSelector((state) => state.ai.mutation)
  const didLoad = useRef(false)

  const load = useCallback(() => dispatch(fetchAiBindings()), [dispatch])

  const save = useCallback(
    async (input) => {
      const result = await dispatch(saveAiBinding(input))
      if (saveAiBinding.rejected.match(result)) return false
      dispatch(fetchAiBindings())
      return true
    },
    [dispatch],
  )
  const remove = useCallback(
    async (serviceName) => {
      const result = await dispatch(removeAiBinding(serviceName))
      if (removeAiBinding.rejected.match(result)) return false
      dispatch(fetchAiBindings())
      return true
    },
    [dispatch],
  )

  useEffect(() => {
    if (didLoad.current) return
    didLoad.current = true
    dispatch(fetchAiBindings())
    dispatch(fetchAiOptions())
    dispatch(fetchAiModelChoices({ ...AI_CHOICES_QUERY, status: AI_STATUS.active }))
  }, [dispatch])

  return {
    data: list.data,
    loading: list.loading,
    success: list.success,
    error: list.error,
    traceId: list.traceId,
    bindableServices: options.data?.bindableServices ?? [],
    models: modelChoices.data,
    mutation,
    load,
    save,
    remove,
  }
}
