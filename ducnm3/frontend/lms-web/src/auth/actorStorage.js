import { ADMIN_ACTOR } from '@/constants/admin'
import { STORAGE_KEYS } from '@/constants/storage'
import {
  readLocalStorage,
  removeLocalStorage,
  writeLocalStorage,
} from '@/utils/localStorage'

export function readActor() {
  const actor = readLocalStorage(STORAGE_KEYS.adminActor)
  if (actor?.type && actor?.id) return { type: actor.type, id: actor.id }

  return initializeActorStorage()
}

export function initializeActorStorage() {
  writeLocalStorage(STORAGE_KEYS.adminActor, ADMIN_ACTOR)
  writeLocalStorage(STORAGE_KEYS.admin, ADMIN_ACTOR)
  return { type: ADMIN_ACTOR.type, id: ADMIN_ACTOR.id }
}

export function writeActor(actor) {
  if (!actor?.type || !actor?.id) throw new Error('A valid actor type and id are required.')
  writeLocalStorage(STORAGE_KEYS.adminActor, { type: actor.type, id: actor.id })
}

export function clearActor() {
  removeLocalStorage(STORAGE_KEYS.adminActor)
}

export function readAdmin() {
  const admin = readLocalStorage(STORAGE_KEYS.admin)
  if (admin?.id && admin?.displayName) return admin

  initializeActorStorage()
  return ADMIN_ACTOR
}
