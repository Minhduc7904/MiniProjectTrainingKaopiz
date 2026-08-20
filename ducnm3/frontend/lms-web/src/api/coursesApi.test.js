import { afterEach, describe, expect, it, vi } from 'vitest'
import { httpClient } from '@/api/httpClient'
import { createCourseRequest } from '@/api/coursesApi'
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
})
