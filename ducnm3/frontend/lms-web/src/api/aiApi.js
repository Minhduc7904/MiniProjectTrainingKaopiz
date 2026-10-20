import { httpClient } from '@/api/httpClient'
import { unwrapEnvelope } from '@/api/unwrapEnvelope'
import { API_ROUTES } from '@/constants/apiRoutes'
import { QUERY_PARAMS } from '@/constants/queryParams'

const PROVIDER_ID_PARAM = 'providerId'

function buildListParams(query = {}, extraKeys = []) {
  const params = {}
  const keys = [
    QUERY_PARAMS.search,
    QUERY_PARAMS.status,
    QUERY_PARAMS.page,
    QUERY_PARAMS.pageSize,
    ...extraKeys,
  ]

  for (const key of keys) {
    if (query[key] !== undefined && query[key] !== null && query[key] !== '') {
      params[key] = query[key]
    }
  }

  return params
}

export async function fetchAiOptionsRequest() {
  const response = await httpClient.get(API_ROUTES.ai.options)
  return unwrapEnvelope(response)
}

export async function fetchAiProvidersRequest(query = {}) {
  const response = await httpClient.get(API_ROUTES.ai.providers, {
    params: buildListParams(query, ['providerType']),
  })
  return unwrapEnvelope(response)
}

export async function createAiProviderRequest(payload) {
  const response = await httpClient.post(API_ROUTES.ai.providers, payload)
  return unwrapEnvelope(response)
}

export async function updateAiProviderRequest(providerId, payload) {
  const response = await httpClient.put(API_ROUTES.ai.providerById(providerId), payload)
  return unwrapEnvelope(response)
}

export async function deleteAiProviderRequest(providerId) {
  await httpClient.delete(API_ROUTES.ai.providerById(providerId))
}

export async function fetchAiModelsRequest(query = {}) {
  const response = await httpClient.get(API_ROUTES.ai.models, {
    params: buildListParams(query, [PROVIDER_ID_PARAM]),
  })
  return unwrapEnvelope(response)
}

export async function createAiModelRequest(payload) {
  const response = await httpClient.post(API_ROUTES.ai.models, payload)
  return unwrapEnvelope(response)
}

export async function updateAiModelRequest(modelId, payload) {
  const response = await httpClient.put(API_ROUTES.ai.modelById(modelId), payload)
  return unwrapEnvelope(response)
}

export async function deleteAiModelRequest(modelId) {
  await httpClient.delete(API_ROUTES.ai.modelById(modelId))
}

export async function fetchAiBindingsRequest() {
  const response = await httpClient.get(API_ROUTES.ai.bindings)
  return unwrapEnvelope(response)
}

export async function upsertAiBindingRequest(serviceName, payload) {
  const response = await httpClient.put(API_ROUTES.ai.bindingByService(serviceName), payload)
  return unwrapEnvelope(response)
}

export async function deleteAiBindingRequest(serviceName) {
  await httpClient.delete(API_ROUTES.ai.bindingByService(serviceName))
}
