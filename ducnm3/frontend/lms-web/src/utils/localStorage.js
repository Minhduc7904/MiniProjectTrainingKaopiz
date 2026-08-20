export function readLocalStorage(key, fallback = null) {
  try {
    const value = localStorage.getItem(key)
    return value == null ? fallback : JSON.parse(value)
  } catch {
    return fallback
  }
}

export function writeLocalStorage(key, value) {
  localStorage.setItem(key, JSON.stringify(value))
  return value
}

export function removeLocalStorage(key) {
  localStorage.removeItem(key)
}
