import { afterEach, describe, expect, it, vi } from 'vitest'
import { httpClient } from '@/api/httpClient'
import { fetchCoursesSummaryRequest, fetchHealthRequest } from '@/api/dashboardApi'
import { API_ROUTES } from '@/constants/apiRoutes'

afterEach(() => vi.restoreAllMocks())

describe('Dashboard requests', () => {
  it('loads the Course summary through the Gateway route', async () => {
    const get = vi.spyOn(httpClient, 'get').mockResolvedValue({
      status: 200,
      data: { data: { totalCourses: 12, totalLessons: 37 }, meta: { traceId: 'trace-course' } },
    })

    await expect(fetchCoursesSummaryRequest()).resolves.toMatchObject({
      data: { totalCourses: 12, totalLessons: 37 }, httpStatus: 200, meta: { traceId: 'trace-course' },
    })
    expect(get).toHaveBeenCalledWith(API_ROUTES.courses.summary)
  })

  it('loads each health check through its declared route', async () => {
    const get = vi.spyOn(httpClient, 'get').mockResolvedValue({
      status: 200,
      data: { data: { service: 'gateway', status: 'healthy' }, meta: { traceId: 'trace-health' } },
    })

    await fetchHealthRequest(API_ROUTES.health.gateway)

    expect(get).toHaveBeenCalledWith(API_ROUTES.health.gateway)
  })
})
