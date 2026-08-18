import { describe, expect, it } from 'vitest'
import { sanitizeApiError } from '@/api/sanitizeApiError'

describe('API error sanitization', () => {
  it('keeps safe error fields and recursively redacts transfer and authentication details', () => {
    const error = sanitizeApiError({
      code: 'VALIDATION_FAILED',
      message: 'Request không hợp lệ.',
      httpStatus: 400,
      traceId: 'trace-1',
      details: {
        uploadUrl: 'https://private.example.test',
        nested: {
          formFields: { policy: 'private-policy' },
          accessToken: 'private-token',
          Cookie: 'session=private-cookie',
        },
        field: 'contentType',
      },
    })

    expect(error).toMatchObject({
      code: 'VALIDATION_FAILED',
      message: 'Request không hợp lệ.',
      httpStatus: 400,
      traceId: 'trace-1',
      details: { field: 'contentType' },
    })
    expect(JSON.stringify(error)).not.toContain('private')
    expect(error.details.uploadUrl).toBe('[REDACTED]')
    expect(error.details.nested.formFields).toBe('[REDACTED]')
    expect(error.details.nested.accessToken).toBe('[REDACTED]')
  })
})
