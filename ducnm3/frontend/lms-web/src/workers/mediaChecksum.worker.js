import { createSHA256 } from 'hash-wasm'
import { API_ERROR_CODES } from '@/constants/apiErrorCodes'

const WORKER_MESSAGES = {
  hash: 'hash',
  cancel: 'cancel',
  progress: 'progress',
  complete: 'complete',
  error: 'error',
}

const DEFAULT_CHUNK_SIZE = 4 * 1024 * 1024
let canceled = false

self.addEventListener('message', async (event) => {
  if (event.data?.type === WORKER_MESSAGES.cancel) {
    canceled = true
    return
  }
  if (event.data?.type !== WORKER_MESSAGES.hash) return

  canceled = false
  const file = event.data.file
  const chunkSize = event.data.chunkSize ?? DEFAULT_CHUNK_SIZE

  try {
    const hash = await createSHA256()
    hash.init()
    let loaded = 0

    while (loaded < file.size) {
      if (canceled) return
      const chunk = file.slice(loaded, Math.min(loaded + chunkSize, file.size))
      hash.update(new Uint8Array(await chunk.arrayBuffer()))
      loaded += chunk.size
      self.postMessage({ type: WORKER_MESSAGES.progress, loaded, total: file.size })
    }

    if (!canceled) {
      self.postMessage({ type: WORKER_MESSAGES.complete, checksum: hash.digest('hex') })
    }
  } catch {
    self.postMessage({
      type: WORKER_MESSAGES.error,
      error: { code: API_ERROR_CODES.checksumFailed, message: 'Không thể tính SHA-256 cho file.' },
    })
  }
})
