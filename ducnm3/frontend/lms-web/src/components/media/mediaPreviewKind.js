export const MEDIA_PREVIEW_KIND = {
  image: 'IMAGE',
  video: 'VIDEO',
  pdf: 'PDF',
  audio: 'AUDIO',
  download: 'DOWNLOAD',
}

export function getMediaPreviewKind(media) {
  if (!media) return MEDIA_PREVIEW_KIND.download
  if (media.mediaType === 'IMAGE') return MEDIA_PREVIEW_KIND.image
  if (media.mediaType === 'VIDEO') return MEDIA_PREVIEW_KIND.video
  if (media.mediaType === 'AUDIO') return MEDIA_PREVIEW_KIND.audio
  if (media.mediaType === 'DOCUMENT' && media.contentType?.toLowerCase().startsWith('application/pdf')) {
    return MEDIA_PREVIEW_KIND.pdf
  }
  return MEDIA_PREVIEW_KIND.download
}
