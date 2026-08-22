const THUMBNAIL_STATUS_LABELS = {
  QUEUED: 'Đang chờ tạo thumbnail',
  PROCESSING: 'Đang tạo thumbnail',
  READY: 'Thumbnail đã sẵn sàng',
  FAILED: 'Tạo thumbnail thất bại',
  NOT_REQUIRED: 'Không yêu cầu thumbnail',
}

export function getMediaLibraryThumbnail(media) {
  if (media?.thumbnail) return media.thumbnail
  if (!media?.thumbnailMediaId) return null

  return {
    id: media.thumbnailMediaId,
    status: media.thumbnailStatus,
    contentUrl: media.thumbnailUrl,
  }
}

export function getThumbnailStatusLabel(status) {
  return THUMBNAIL_STATUS_LABELS[status] || status || THUMBNAIL_STATUS_LABELS.NOT_REQUIRED
}

export function hasReadyMediaLibraryThumbnail(media) {
  const thumbnail = getMediaLibraryThumbnail(media)
  return thumbnail?.status === 'READY' && Boolean(thumbnail.contentUrl)
}
