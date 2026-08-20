import { File, FileText, Image, Music2, Video } from 'lucide-react'
import { MEDIA_TYPE_LABELS } from '@/constants/media'

const MEDIA_TYPE_ICONS = {
  IMAGE: Image,
  VIDEO: Video,
  DOCUMENT: FileText,
  AUDIO: Music2,
  OTHER: File,
}

export function getMediaTypeIcon(mediaType) {
  return MEDIA_TYPE_ICONS[mediaType] ?? File
}

export function getMediaTypeLabel(mediaType) {
  return MEDIA_TYPE_LABELS[mediaType] ?? MEDIA_TYPE_LABELS.OTHER
}
