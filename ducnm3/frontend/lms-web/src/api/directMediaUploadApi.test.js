import { afterEach, describe, expect, it, vi } from 'vitest'
import { httpClient } from '@/api/httpClient'
import {
  buildSignedUploadForm,
  completeDirectUploadRequest,
  createUploadIntentRequest,
  uploadToSignedUrl,
} from '@/api/directMediaUploadApi'
import {
  getMediaContentRequest,
  getMediaThumbnailRequest,
  retryMediaThumbnailRequest,
} from '@/api/mediaApi'

afterEach(() => vi.restoreAllMocks())

describe('Media Service direct upload requests', () => {
  it('loads image content through the configured API client as a blob', async () => {
    const content = new Blob(['image-bytes'], { type: 'image/png' })
    const get = vi.spyOn(httpClient, 'get').mockResolvedValue({ data: content })

    await expect(getMediaContentRequest('/media/api/media/media-1/content')).resolves.toBe(content)
    expect(get).toHaveBeenCalledWith('/media/api/media/media-1/content', {
      signal: undefined,
      responseType: 'blob',
    })
  })

  it('gets thumbnail status from the public status URL', async () => {
    const get = vi.spyOn(httpClient, 'get').mockResolvedValue({
      data: { data: { status: 'PROCESSING' }, meta: { traceId: 'trace-3' } },
    })

    await expect(getMediaThumbnailRequest('/media/api/media/media-1/thumbnail')).resolves.toMatchObject({
      data: { status: 'PROCESSING' },
    })
    expect(get).toHaveBeenCalledWith('/media/api/media/media-1/thumbnail', { signal: undefined })
  })

  it('retries a failed thumbnail with the retry actor', async () => {
    const post = vi.spyOn(httpClient, 'post').mockResolvedValue({
      data: { data: { status: 'QUEUED' }, meta: { traceId: 'trace-4' } },
    })

    await retryMediaThumbnailRequest('media-1', {
      requestedByType: 'STUDENT',
      requestedBy: 'actor-1',
    })

    expect(post).toHaveBeenCalledWith(
      expect.stringContaining('media-1/thumbnail/retry'),
      { requestedByType: 'STUDENT', requestedBy: 'actor-1' },
      { signal: undefined },
    )
  })

  it('forwards AbortSignal to intent creation', async () => {
    const controller = new AbortController()
    const post = vi.spyOn(httpClient, 'post').mockResolvedValue({
      data: { data: { mediaId: 'media-1' }, meta: { traceId: 'trace-1' } },
    })

    await createUploadIntentRequest({ originalFileName: 'lesson.mp4' }, { signal: controller.signal })

    expect(post).toHaveBeenCalledWith(
      expect.any(String),
      { originalFileName: 'lesson.mp4' },
      { signal: controller.signal },
    )
  })

  it('forwards AbortSignal to finalization', async () => {
    const controller = new AbortController()
    const post = vi.spyOn(httpClient, 'post').mockResolvedValue({
      data: { data: { id: 'media-1', status: 'READY' }, meta: { traceId: 'trace-2' } },
    })

    await completeDirectUploadRequest(
      'media-1',
      { uploadedBy: 'actor-1', uploadedByType: 'STUDENT' },
      { signal: controller.signal },
    )

    expect(post).toHaveBeenCalledWith(
      expect.stringContaining('media-1'),
      { uploadedBy: 'actor-1', uploadedByType: 'STUDENT' },
      { signal: controller.signal },
    )
  })
})

describe('signed direct upload', () => {
  it('builds returned form fields in order and appends the file last', () => {
    const file = new File(['lesson'], 'lesson.mp4', { type: 'video/mp4' })
    const body = buildSignedUploadForm(
      { key: 'media/key', policy: 'signed-policy', 'x-amz-signature': 'secret' },
      file,
    )

    expect([...body.keys()]).toEqual(['key', 'policy', 'x-amz-signature', 'file'])
    expect(body.get('file')).toBe(file)
  })

  it('uses raw XHR for the signed URL and reports upload progress', async () => {
    const events = []
    const xhr = {
      upload: {},
      open: vi.fn(),
      send: vi.fn(function send() {
        this.upload.onprogress({ lengthComputable: true, loaded: 1, total: 2 })
        this.status = 204
        this.onload()
      }),
      abort: vi.fn(),
      status: 0,
    }
    const file = new File(['lesson'], 'lesson.txt', { type: 'text/plain' })

    await uploadToSignedUrl({
      uploadUrl: 'https://storage.example.test/upload',
      formFields: { key: 'media/key' },
      file,
      onProgress: (progress) => events.push(progress),
      createXhr: () => xhr,
    })

    expect(xhr.open).toHaveBeenCalledWith('POST', 'https://storage.example.test/upload')
    expect(xhr.send).toHaveBeenCalledOnce()
    expect(events).toEqual([50])
  })

  it('rejects without including signed URL or form fields in the error', async () => {
    const xhr = {
      upload: {},
      open: vi.fn(),
      send() {
        this.status = 403
        this.onload()
      },
      abort: vi.fn(),
      status: 0,
    }

    await expect(
      uploadToSignedUrl({
        uploadUrl: 'https://storage.example.test/private-signature',
        formFields: { policy: 'private-policy' },
        file: new File(['x'], 'x.txt', { type: 'text/plain' }),
        createXhr: () => xhr,
      }),
    ).rejects.toMatchObject({ code: 'DIRECT_UPLOAD_FAILED', httpStatus: 403 })
  })

  it('aborts the raw XHR and rejects with a safe cancellation error', async () => {
    const xhr = {
      upload: {},
      open: vi.fn(),
      send: vi.fn(),
      abort: vi.fn(),
      status: 0,
    }
    const request = uploadToSignedUrl({
      uploadUrl: 'https://storage.example.test/private-signature',
      formFields: { policy: 'private-policy' },
      file: new File(['x'], 'x.txt', { type: 'text/plain' }),
      createXhr: () => xhr,
    })

    request.abort()

    expect(xhr.abort).toHaveBeenCalledOnce()
    await expect(request).rejects.toMatchObject({ code: 'UPLOAD_CANCELED' })
    expect(xhr.onload).toBeNull()
    expect(xhr.onerror).toBeNull()
    expect(xhr.onabort).toBeNull()
    expect(xhr.ontimeout).toBeNull()
    expect(xhr.upload.onprogress).toBeNull()
  })

  it('uses the configured timeout and rejects safely when MinIO times out', async () => {
    const xhr = {
      upload: {},
      open: vi.fn(),
      send() {
        this.ontimeout()
      },
      abort: vi.fn(),
      timeout: 0,
      status: 0,
    }

    const request = uploadToSignedUrl({
      uploadUrl: 'https://storage.example.test/private-signature',
      formFields: { policy: 'private-policy' },
      file: new File(['x'], 'x.txt', { type: 'text/plain' }),
      timeoutMs: 1234,
      createXhr: () => xhr,
    })

    await expect(request).rejects.toMatchObject({ code: 'REQUEST_TIMEOUT' })
    expect(xhr.timeout).toBe(1234)
    expect(xhr.onload).toBeNull()
    expect(xhr.onerror).toBeNull()
    expect(xhr.onabort).toBeNull()
    expect(xhr.ontimeout).toBeNull()
    expect(xhr.upload.onprogress).toBeNull()
  })

  it('converts synchronous XHR setup failures to a fixed safe error and clears handlers', async () => {
    const xhr = {
      upload: {},
      open() {
        throw new Error('private signed URL leaked by browser')
      },
      send: vi.fn(),
      abort: vi.fn(),
      status: 0,
    }

    const request = uploadToSignedUrl({
      uploadUrl: 'https://storage.example.test/private-signature',
      formFields: { policy: 'private-policy' },
      file: new File(['x'], 'x.txt', { type: 'text/plain' }),
      createXhr: () => xhr,
    })

    await expect(request).rejects.toMatchObject({
      code: 'DIRECT_UPLOAD_FAILED',
      message: 'Không thể bắt đầu direct upload tới storage.',
    })
    expect(JSON.stringify(await request.catch((error) => error))).not.toContain('private')
    expect(xhr.onload).toBeNull()
    expect(xhr.upload.onprogress).toBeNull()
  })
})
