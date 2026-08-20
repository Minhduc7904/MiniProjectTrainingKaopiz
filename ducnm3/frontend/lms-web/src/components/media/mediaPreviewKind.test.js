import { describe, expect, it } from 'vitest'
import { getMediaPreviewKind, MEDIA_PREVIEW_KIND } from '@/components/media/mediaPreviewKind'

describe('getMediaPreviewKind', () => {
  it.each([
    [{ mediaType: 'IMAGE' }, MEDIA_PREVIEW_KIND.image],
    [{ mediaType: 'VIDEO' }, MEDIA_PREVIEW_KIND.video],
    [{ mediaType: 'DOCUMENT', contentType: 'application/pdf' }, MEDIA_PREVIEW_KIND.pdf],
    [{ mediaType: 'DOCUMENT', contentType: 'application/pdf; charset=binary' }, MEDIA_PREVIEW_KIND.pdf],
    [{ mediaType: 'AUDIO' }, MEDIA_PREVIEW_KIND.audio],
    [{ mediaType: 'DOCUMENT', contentType: 'text/plain' }, MEDIA_PREVIEW_KIND.download],
    [{ mediaType: 'OTHER' }, MEDIA_PREVIEW_KIND.download],
  ])('maps %o to %s', (media, expected) => {
    expect(getMediaPreviewKind(media)).toBe(expected)
  })
})
