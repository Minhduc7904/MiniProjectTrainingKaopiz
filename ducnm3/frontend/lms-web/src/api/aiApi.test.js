import { afterEach, describe, expect, it, vi } from 'vitest'
import {
  createAiProviderRequest,
  deleteAiBindingRequest,
  deleteAiProviderRequest,
  fetchAiModelsRequest,
  fetchAiProvidersRequest,
  updateAiProviderRequest,
  upsertAiBindingRequest,
} from '@/api/aiApi'
import { httpClient } from '@/api/httpClient'
import { API_ROUTES } from '@/constants/apiRoutes'

afterEach(() => vi.restoreAllMocks())

describe('AI provider requests', () => {
  it('drops empty filters and keeps pagination for the provider list', async () => {
    const get = vi.spyOn(httpClient, 'get').mockResolvedValue({
      data: { data: [], meta: { traceId: 'trace-ai', pagination: {} } },
    })

    await fetchAiProvidersRequest({ search: '', status: 'ACTIVE', providerType: undefined, page: 2, pageSize: 20 })

    expect(get).toHaveBeenCalledWith(API_ROUTES.ai.providers, {
      params: { status: 'ACTIVE', page: 2, pageSize: 20 },
    })
  })

  it('posts the create payload to the provider route', async () => {
    const post = vi.spyOn(httpClient, 'post').mockResolvedValue({ data: { data: { id: 'p1' }, meta: {} } })
    const payload = { name: 'OpenAI', providerType: 'OPENAI', apiKey: 'sk-test' }

    const result = await createAiProviderRequest(payload)

    expect(post).toHaveBeenCalledWith(API_ROUTES.ai.providers, payload)
    expect(result.data).toEqual({ id: 'p1' })
  })

  it('puts updates and deletes by provider id under the gateway prefix', async () => {
    const put = vi.spyOn(httpClient, 'put').mockResolvedValue({ data: { data: {}, meta: {} } })
    const remove = vi.spyOn(httpClient, 'delete').mockResolvedValue({})

    await updateAiProviderRequest('p1', { name: 'Renamed' })
    await deleteAiProviderRequest('p1')

    expect(API_ROUTES.ai.providerById('p1')).toBe('/ai/api/ai/providers/p1')
    expect(put).toHaveBeenCalledWith('/ai/api/ai/providers/p1', { name: 'Renamed' })
    expect(remove).toHaveBeenCalledWith('/ai/api/ai/providers/p1')
  })
})

describe('AI model and binding requests', () => {
  it('filters models by providerId', async () => {
    const get = vi.spyOn(httpClient, 'get').mockResolvedValue({ data: { data: [], meta: {} } })

    await fetchAiModelsRequest({ providerId: 'p1', page: 1, pageSize: 100 })

    expect(get).toHaveBeenCalledWith(API_ROUTES.ai.models, {
      params: { page: 1, pageSize: 100, providerId: 'p1' },
    })
  })

  it('upserts and removes a binding by service name', async () => {
    const put = vi.spyOn(httpClient, 'put').mockResolvedValue({ data: { data: {}, meta: {} } })
    const remove = vi.spyOn(httpClient, 'delete').mockResolvedValue({})

    await upsertAiBindingRequest('course-service', { modelId: 'm1' })
    await deleteAiBindingRequest('course-service')

    expect(put).toHaveBeenCalledWith('/ai/api/ai/service-bindings/course-service', { modelId: 'm1' })
    expect(remove).toHaveBeenCalledWith('/ai/api/ai/service-bindings/course-service')
  })
})
