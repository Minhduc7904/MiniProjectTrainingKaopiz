import { createAsyncThunk, createSlice } from '@reduxjs/toolkit'
import {
  checkAiProviderHealthRequest,
  checkAiProvidersHealthRequest,
  createAiModelRequest,
  createAiProviderRequest,
  deleteAiBindingRequest,
  deleteAiModelRequest,
  deleteAiProviderRequest,
  fetchAiBindingsRequest,
  fetchAiModelsRequest,
  fetchAiOptionsRequest,
  fetchAiProvidersRequest,
  updateAiModelRequest,
  updateAiProviderRequest,
  upsertAiBindingRequest,
} from '@/api/aiApi'
import { toApiError } from '@/api/toApiError'
import { API_ERROR_CODES } from '@/constants/apiErrorCodes'
import { AI_LIST_DEFAULT_QUERY } from '@/constants/ai'
import {
  assignListQuery,
  createInitialListState,
  listRequestFulfilled,
  listRequestPending,
  listRequestRejected,
} from '@/features/createApiListSlice'

const MUTATION_PREFIX = 'ai/mutate/'

function createReadThunk(type, request) {
  return createAsyncThunk(type, async (arg, { rejectWithValue }) => {
    try {
      const { data, meta } = await request(arg)
      return { data, pagination: meta.pagination, traceId: meta.traceId }
    } catch (error) {
      return rejectWithValue(toApiError(error))
    }
  })
}

function createMutationThunk(name, request) {
  return createAsyncThunk(`${MUTATION_PREFIX}${name}`, async (arg, { rejectWithValue }) => {
    try {
      const result = await request(arg)
      return { data: result?.data ?? null, traceId: result?.meta?.traceId ?? null }
    } catch (error) {
      return rejectWithValue(toApiError(error))
    }
  })
}

export const fetchAiOptions = createReadThunk('ai/fetchOptions', fetchAiOptionsRequest)
export const fetchAiProviders = createReadThunk('ai/fetchProviders', fetchAiProvidersRequest)
export const fetchAiProviderChoices = createReadThunk('ai/fetchProviderChoices', fetchAiProvidersRequest)
export const fetchAiModels = createReadThunk('ai/fetchModels', fetchAiModelsRequest)
export const fetchAiModelChoices = createReadThunk('ai/fetchModelChoices', fetchAiModelsRequest)
export const fetchAiBindings = createReadThunk('ai/fetchBindings', fetchAiBindingsRequest)

export const checkAiProvidersHealth = createAsyncThunk('ai/checkProvidersHealth', async (_, { rejectWithValue }) => {
  try {
    const { data } = await checkAiProvidersHealthRequest()
    return { items: data ?? [] }
  } catch (error) {
    return rejectWithValue(toApiError(error))
  }
})

export const checkAiProviderHealth = createAsyncThunk('ai/checkProviderHealth', async (providerId, { rejectWithValue }) => {
  try {
    const { data } = await checkAiProviderHealthRequest(providerId)
    return { items: [data] }
  } catch (error) {
    return rejectWithValue(toApiError(error))
  }
})

export const saveAiProvider = createMutationThunk('saveProvider', ({ id, payload }) =>
  id ? updateAiProviderRequest(id, payload) : createAiProviderRequest(payload),
)
export const removeAiProvider = createMutationThunk('removeProvider', deleteAiProviderRequest)
export const saveAiModel = createMutationThunk('saveModel', ({ id, payload }) =>
  id ? updateAiModelRequest(id, payload) : createAiModelRequest(payload),
)
export const removeAiModel = createMutationThunk('removeModel', deleteAiModelRequest)
export const saveAiBinding = createMutationThunk('saveBinding', ({ serviceName, payload }) =>
  upsertAiBindingRequest(serviceName, payload),
)
export const removeAiBinding = createMutationThunk('removeBinding', deleteAiBindingRequest)

function createChoicesState() {
  return { data: [], loading: false, error: null }
}

function isMutationAction(suffix) {
  return (action) =>
    typeof action.type === 'string' &&
    action.type.startsWith(MUTATION_PREFIX) &&
    action.type.endsWith(suffix)
}

const aiSlice = createSlice({
  name: 'ai',
  initialState: {
    options: { data: null, loading: false, error: null },
    providers: createInitialListState(AI_LIST_DEFAULT_QUERY),
    models: createInitialListState(AI_LIST_DEFAULT_QUERY),
    bindings: createInitialListState({}),
    providerChoices: createChoicesState(),
    modelChoices: createChoicesState(),
    // Kết quả thử kết nối theo providerId; không lưu DB nên mỗi lần mở trang được thử lại.
    health: { byId: {}, loadingAll: false, checkingIds: [], error: null },
    mutation: { loading: false, success: false, error: null, traceId: null },
  },
  reducers: {
    setAiProvidersQuery(state, action) {
      assignListQuery(state.providers, action.payload)
    },
    setAiModelsQuery(state, action) {
      assignListQuery(state.models, action.payload)
    },
  },
  extraReducers: (builder) => {
    builder
      .addCase(fetchAiOptions.pending, (state) => {
        state.options.loading = true
        state.options.error = null
      })
      .addCase(fetchAiOptions.fulfilled, (state, action) => {
        state.options.loading = false
        state.options.data = action.payload.data
      })
      .addCase(fetchAiOptions.rejected, (state, action) => {
        state.options.loading = false
        state.options.error = action.payload ?? { code: API_ERROR_CODES.unexpectedError }
      })
      .addCase(fetchAiProviders.pending, (state, action) => listRequestPending(state.providers, action))
      .addCase(fetchAiProviders.fulfilled, (state, action) => listRequestFulfilled(state.providers, action))
      .addCase(fetchAiProviders.rejected, (state, action) => listRequestRejected(state.providers, action))
      .addCase(fetchAiModels.pending, (state, action) => listRequestPending(state.models, action))
      .addCase(fetchAiModels.fulfilled, (state, action) => listRequestFulfilled(state.models, action))
      .addCase(fetchAiModels.rejected, (state, action) => listRequestRejected(state.models, action))
      .addCase(fetchAiBindings.pending, (state, action) => listRequestPending(state.bindings, action))
      .addCase(fetchAiBindings.fulfilled, (state, action) => listRequestFulfilled(state.bindings, action))
      .addCase(fetchAiBindings.rejected, (state, action) => listRequestRejected(state.bindings, action))
      .addCase(checkAiProvidersHealth.pending, (state) => {
        state.health.loadingAll = true
        state.health.error = null
      })
      .addCase(checkAiProvidersHealth.fulfilled, (state, action) => {
        state.health.loadingAll = false
        state.health.byId = Object.fromEntries(action.payload.items.map((item) => [item.providerId, item]))
      })
      .addCase(checkAiProvidersHealth.rejected, (state, action) => {
        state.health.loadingAll = false
        state.health.error = action.payload ?? { code: API_ERROR_CODES.unexpectedError }
      })
      .addCase(checkAiProviderHealth.pending, (state, action) => {
        state.health.checkingIds.push(action.meta.arg)
      })
      .addCase(checkAiProviderHealth.fulfilled, (state, action) => {
        state.health.checkingIds = state.health.checkingIds.filter((id) => id !== action.meta.arg)
        for (const item of action.payload.items) state.health.byId[item.providerId] = item
      })
      .addCase(checkAiProviderHealth.rejected, (state, action) => {
        state.health.checkingIds = state.health.checkingIds.filter((id) => id !== action.meta.arg)
        state.health.error = action.payload ?? { code: API_ERROR_CODES.unexpectedError }
      })
      .addCase(fetchAiProviderChoices.pending, (state) => {
        state.providerChoices.loading = true
      })
      .addCase(fetchAiProviderChoices.fulfilled, (state, action) => {
        state.providerChoices = { data: action.payload.data ?? [], loading: false, error: null }
      })
      .addCase(fetchAiProviderChoices.rejected, (state, action) => {
        state.providerChoices = { data: [], loading: false, error: action.payload ?? null }
      })
      .addCase(fetchAiModelChoices.pending, (state) => {
        state.modelChoices.loading = true
      })
      .addCase(fetchAiModelChoices.fulfilled, (state, action) => {
        state.modelChoices = { data: action.payload.data ?? [], loading: false, error: null }
      })
      .addCase(fetchAiModelChoices.rejected, (state, action) => {
        state.modelChoices = { data: [], loading: false, error: action.payload ?? null }
      })
      .addMatcher(isMutationAction('/pending'), (state) => {
        state.mutation = { loading: true, success: false, error: null, traceId: null }
      })
      .addMatcher(isMutationAction('/fulfilled'), (state, action) => {
        state.mutation = {
          loading: false,
          success: true,
          error: null,
          traceId: action.payload?.traceId ?? null,
        }
      })
      .addMatcher(isMutationAction('/rejected'), (state, action) => {
        state.mutation = {
          loading: false,
          success: false,
          error: action.payload ?? {
            code: API_ERROR_CODES.unexpectedError,
            message: 'Đã xảy ra lỗi không xác định.',
          },
          traceId: action.payload?.traceId ?? null,
        }
      })
  },
})

export const { setAiProvidersQuery, setAiModelsQuery } = aiSlice.actions
export const aiReducer = aiSlice.reducer
