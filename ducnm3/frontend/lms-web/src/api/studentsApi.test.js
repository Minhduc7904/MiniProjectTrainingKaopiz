import { afterEach, describe, expect, it, vi } from 'vitest'
import { httpClient } from '@/api/httpClient'
import { fetchStudentsListRequest } from '@/api/studentsApi'
import { API_ROUTES } from '@/constants/apiRoutes'

afterEach(() => vi.restoreAllMocks())

describe('Student list requests', () => {
  it('sends the search term through the Student list route', async () => {
    const get = vi.spyOn(httpClient, 'get').mockResolvedValue({
      data: { data: [], meta: { traceId: 'trace-students', pagination: {} } },
    })

    await fetchStudentsListRequest({ search: 'student@example.com', page: 1, pageSize: 20 })

    expect(get).toHaveBeenCalledWith(API_ROUTES.students.list, {
      params: { search: 'student@example.com', page: 1, pageSize: 20 },
    })
  })
})
