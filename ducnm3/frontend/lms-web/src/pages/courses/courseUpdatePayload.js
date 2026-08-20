export function buildChangedPayload(initial, draft) {
  return Object.fromEntries(
    Object.entries(draft).filter(([key, value]) => value !== initial[key]),
  )
}
