import { describe, expect, it } from 'vitest'
import {
  isImageMedia,
  shouldPollThumbnail,
  toThumbnailRetryActor,
} from '@/hooks/media/mediaThumbnail'

describe('media thumbnail helpers', () => {
  it('polls only active thumbnail jobs that expose a status URL', () => {
    expect(shouldPollThumbnail({ status: 'QUEUED', thumbnailStatusUrl: '/thumbnail' })).toBe(true)
    expect(shouldPollThumbnail({ status: 'PROCESSING', thumbnailStatusUrl: '/thumbnail' })).toBe(true)
    expect(shouldPollThumbnail({ status: 'READY', thumbnailStatusUrl: '/thumbnail' })).toBe(false)
    expect(shouldPollThumbnail({ status: 'QUEUED', thumbnailStatusUrl: null })).toBe(false)
  })

  it('recognizes only image media for content preview', () => {
    expect(isImageMedia({ mediaType: 'IMAGE', contentType: 'image/png' })).toBe(true)
    expect(isImageMedia({ mediaType: 'DOCUMENT', contentType: 'application/pdf' })).toBe(false)
  })

  it('maps the upload actor to the thumbnail retry contract', () => {
    expect(toThumbnailRetryActor({ uploadedByType: 'STUDENT', uploadedBy: 'actor-1' })).toEqual({
      requestedByType: 'STUDENT',
      requestedBy: 'actor-1',
    })
  })
})
