let verifiedStudentId = null

export function isStudentSessionVerified(actor) {
  return actor?.id === verifiedStudentId
}

export function verifyStudentSession(studentId) {
  verifiedStudentId = studentId
}

export function clearStudentSession() {
  verifiedStudentId = null
}
