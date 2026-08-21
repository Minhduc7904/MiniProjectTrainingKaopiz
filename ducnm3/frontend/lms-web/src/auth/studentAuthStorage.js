import { STORAGE_KEYS } from '@/constants/storage'
import { readLocalStorage, removeLocalStorage, writeLocalStorage } from '@/utils/localStorage'
import { clearStudentSession } from '@/auth/studentSession'

export function readStudentActor() {
  const actor = readLocalStorage(STORAGE_KEYS.studentActor)
  return actor?.type === 'STUDENT' && typeof actor.id === 'string' ? actor : null
}

export function writeStudentActor(id) {
  if (!id) throw new Error('Student ID is required.')
  return writeLocalStorage(STORAGE_KEYS.studentActor, { type: 'STUDENT', id })
}

export function clearStudentActor() {
  clearStudentSession()
  removeLocalStorage(STORAGE_KEYS.studentActor)
}
