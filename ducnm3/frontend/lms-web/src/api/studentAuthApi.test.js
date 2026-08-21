import { afterEach, describe, expect, it, vi } from 'vitest'

const { post } = vi.hoisted(() => ({ post: vi.fn() }))

vi.mock('axios', () => ({
  default: {
    create: () => ({
      post,
      get: vi.fn(),
      interceptors: {
        request: { use: vi.fn() },
        response: { use: vi.fn() },
      },
    }),
  },
}))

import { studentAuthApi } from '@/api/studentAuthApi'

afterEach(() => {
  post.mockReset()
})

describe('studentAuthApi', () => {
  it('returns the Student actor payload from the register response envelope', async () => {
    post.mockResolvedValue({
      data: {
        data: { actor: 'STUDENT', id: '11111111-1111-1111-1111-111111111111' },
        meta: { traceId: 'trace-1' },
      },
    })

    await expect(studentAuthApi.register({ email: 'student@example.com', displayName: 'Student' }))
      .resolves.toEqual({ actor: 'STUDENT', id: '11111111-1111-1111-1111-111111111111' })
  })
})
