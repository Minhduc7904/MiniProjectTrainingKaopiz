import { describe, expect, it } from 'vitest'
import { File, FileText, Image, Music2, Video } from 'lucide-react'
import { getMediaTypeIcon, getMediaTypeLabel } from '@/components/media/mediaPresentation'

describe('media library presentation', () => {
  it.each([
    ['IMAGE', Image, 'Ảnh'],
    ['VIDEO', Video, 'Video'],
    ['DOCUMENT', FileText, 'Tài liệu'],
    ['AUDIO', Music2, 'Audio'],
    ['OTHER', File, 'Khác'],
  ])('maps %s to its icon and label', (mediaType, icon, label) => {
    expect(getMediaTypeIcon(mediaType)).toBe(icon)
    expect(getMediaTypeLabel(mediaType)).toBe(label)
  })
})
