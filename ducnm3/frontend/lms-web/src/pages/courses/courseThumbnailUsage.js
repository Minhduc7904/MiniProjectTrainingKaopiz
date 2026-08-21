export function buildCourseThumbnailUsage(courseId, selectedMedia) {
  const mediaId = selectedMedia[0]?.id
  if (!mediaId) return null

  return {
    mediaId,
    ownerService: 'COURSE',
    ownerType: 'COURSE_THUMBNAIL',
    ownerId: courseId,
    usageType: 'THUMBNAIL',
    displayOrder: 0,
  }
}
