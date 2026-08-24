import { afterEach, describe, expect, it, vi } from 'vitest'
import { httpClient } from '@/api/httpClient'
import { createCourseRequest, fetchCoursesListRequest } from '@/api/coursesApi'
import { API_ROUTES } from '@/constants/apiRoutes'

afterEach(() => vi.restoreAllMocks())

describe('Course requests', () => {
  it('creates a course through the gateway list route', async () => {
    const payload = {
      name: 'Backend Fundamentals',
      descriptionMarkdown: '![cover](/media/api/media/media-1/content)',
    }
    const post = vi.spyOn(httpClient, 'post').mockResolvedValue({
      data: { data: { id: 'course-1', status: 'DRAFT' }, meta: { traceId: 'trace-1' } },
    })

    await expect(createCourseRequest(payload)).resolves.toMatchObject({
      data: { id: 'course-1', status: 'DRAFT' },
    })
    expect(post).toHaveBeenCalledWith(API_ROUTES.courses.list, payload)
  })

  it('sends the course search query through the gateway list route', async () => {
    const get = vi.spyOn(httpClient, 'get').mockResolvedValue({
      data: { data: [], meta: { traceId: 'trace-2', pagination: {} } },
    })

    await fetchCoursesListRequest({ search: 'Backend', page: 1, pageSize: 20 })

    expect(get).toHaveBeenCalledWith(API_ROUTES.courses.list, {
      params: { search: 'Backend', page: 1, pageSize: 20 },
    })
  })
})
