import { afterEach, describe, expect, it, vi } from 'vitest'
import { httpClient } from '@/api/httpClient'
import {
  createLessonAiJobRequest,
  fetchLessonAiDocumentsRequest,
  fetchLessonAiJobsRequest,
  syncLessonAiDocumentsRequest,
} from '@/api/lessonAiApi'
import { API_ROUTES } from '@/constants/apiRoutes'
import { SILENT_TOAST_CONFIG } from '@/constants/toast'

afterEach(() => vi.restoreAllMocks())

const envelope = (data) => ({ data: { data, meta: { traceId: 't' } } })

describe('lesson AI requests', () => {
  it('reads documents with a toast by default and silently while polling', async () => {
    const get = vi.spyOn(httpClient, 'get').mockResolvedValue(envelope({ items: [] }))

    await fetchLessonAiDocumentsRequest('c1', 'l1')
    await fetchLessonAiDocumentsRequest('c1', 'l1', { silent: true })

    expect(get).toHaveBeenNthCalledWith(1, API_ROUTES.courses.lessonAiDocuments('c1', 'l1'), { signal: undefined })
    expect(get).toHaveBeenNthCalledWith(2, API_ROUTES.courses.lessonAiDocuments('c1', 'l1'), { signal: undefined, apiToast: SILENT_TOAST_CONFIG })
  })

  it('syncs documents and only sends retryFailed when asked', async () => {
    const post = vi.spyOn(httpClient, 'post').mockResolvedValue(envelope({ items: [] }))

    await syncLessonAiDocumentsRequest('c1', 'l1')
    await syncLessonAiDocumentsRequest('c1', 'l1', { retryFailed: true })

    expect(post).toHaveBeenNthCalledWith(1, API_ROUTES.courses.lessonAiDocumentsSync('c1', 'l1'), null, { params: undefined, signal: undefined })
    expect(post).toHaveBeenNthCalledWith(2, API_ROUTES.courses.lessonAiDocumentsSync('c1', 'l1'), null, { params: { retryFailed: true }, signal: undefined })
  })

  it('syncs silently when it runs as a background refresh', async () => {
    const post = vi.spyOn(httpClient, 'post').mockResolvedValue(envelope({ items: [] }))

    await syncLessonAiDocumentsRequest('c1', 'l1', { silent: true })

    expect(post).toHaveBeenCalledWith(API_ROUTES.courses.lessonAiDocumentsSync('c1', 'l1'), null, { params: undefined, signal: undefined, apiToast: SILENT_TOAST_CONFIG })
  })

  it('posts the job payload to the jobs route and lists jobs from the same route', async () => {
    const post = vi.spyOn(httpClient, 'post').mockResolvedValue(envelope({ id: 'j1' }))
    const get = vi.spyOn(httpClient, 'get').mockResolvedValue(envelope({ items: [] }))
    const payload = { jobType: 'GENERATE_QUIZ', mediaIds: ['m1'], questionCount: 5 }

    const created = await createLessonAiJobRequest('c1', 'l1', payload)
    await fetchLessonAiJobsRequest('c1', 'l1', { silent: true })

    expect(post).toHaveBeenCalledWith(API_ROUTES.courses.lessonAiJobs('c1', 'l1'), payload)
    expect(created.data).toEqual({ id: 'j1' })
    expect(get).toHaveBeenCalledWith(API_ROUTES.courses.lessonAiJobs('c1', 'l1'), { signal: undefined, apiToast: SILENT_TOAST_CONFIG })
  })
})
