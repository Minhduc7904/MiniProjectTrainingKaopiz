import { afterEach, describe, expect, it } from 'vitest'
import {
  clearStudentSession,
  getVerifiedStudentProfile,
  verifyStudentSession,
} from '@/auth/studentSession'

afterEach(clearStudentSession)

describe('student session', () => {
  it('keeps the verified Student profile in memory for the protected header', () => {
    const profile = {
      displayName: 'Student One',
      email: 'student@example.com',
      id: '11111111-1111-1111-1111-111111111111',
      status: 'ACTIVE',
    }

    verifyStudentSession(profile)

    expect(getVerifiedStudentProfile()).toEqual(profile)
  })
})
