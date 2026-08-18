import { describe, expect, it } from 'vitest'
import { redactHttpLogValue } from '@/api/httpLogger'

describe('HTTP log redaction', () => {
  it('redacts signed upload material recursively without changing safe fields', () => {
    const value = redactHttpLogValue({
      data: {
        id: 'media-1',
        uploadUrl: 'https://private.example.test',
        formFields: {
          key: 'safe-routing-is-still-sensitive-in-form-fields',
          policy: 'private-policy',
          nested: { 'X-Amz-Credential': 'private-credential' },
        },
      },
      signature: 'private-signature',
      credential: 'private-credential',
      securityToken: 'private-token',
    })

    expect(value.data.id).toBe('media-1')
    expect(JSON.stringify(value)).not.toContain('private')
    expect(value.data.uploadUrl).toBe('[REDACTED]')
    expect(value.data.formFields).toBe('[REDACTED]')
  })

  it('redacts every x-amz field regardless of casing', () => {
    expect(
      redactHttpLogValue({
        'x-amz-algorithm': 'algorithm',
        'X-Amz-Signature': 'signature',
      }),
    ).toEqual({
      'x-amz-algorithm': '[REDACTED]',
      'X-Amz-Signature': '[REDACTED]',
    })
  })

  it('redacts authentication and cookie headers recursively', () => {
    const value = redactHttpLogValue({
      headers: {
        Authorization: 'Bearer private-token',
        'Proxy-Authorization': 'Basic private-credential',
        Cookie: 'session=private-cookie',
        'Set-Cookie': ['session=private-cookie'],
        Accept: 'application/json',
      },
    })

    expect(value.headers).toEqual({
      Authorization: '[REDACTED]',
      'Proxy-Authorization': '[REDACTED]',
      Cookie: '[REDACTED]',
      'Set-Cookie': '[REDACTED]',
      Accept: 'application/json',
    })
  })
})
