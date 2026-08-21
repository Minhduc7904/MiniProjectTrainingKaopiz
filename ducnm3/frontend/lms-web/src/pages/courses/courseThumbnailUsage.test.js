import { describe, expect, it } from 'vitest'
import { buildCourseThumbnailUsage } from '@/pages/courses/courseThumbnailUsage'

describe('buildCourseThumbnailUsage', () => {
  it('uses the selected READY original media ID instead of its thumbnail derivative ID', () => {
    expect(buildCourseThumbnailUsage(
      'course-1',
      [{ id: 'original-image', thumbnailMediaId: 'derived-webp' }],
    )).toEqual({
      mediaId: 'original-image',
      ownerService: 'COURSE',
      ownerType: 'COURSE_THUMBNAIL',
      ownerId: 'course-1',
      usageType: 'THUMBNAIL',
      displayOrder: 0,
    })
  })
})
