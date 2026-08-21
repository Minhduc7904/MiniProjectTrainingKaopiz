let verifiedStudentProfile = null

export function isStudentSessionVerified(actor) {
  return actor?.id === verifiedStudentProfile?.id
}

export function verifyStudentSession(student) {
  verifiedStudentProfile = student
}

export function getVerifiedStudentProfile() {
  return verifiedStudentProfile
}

export function clearStudentSession() {
  verifiedStudentProfile = null
}
