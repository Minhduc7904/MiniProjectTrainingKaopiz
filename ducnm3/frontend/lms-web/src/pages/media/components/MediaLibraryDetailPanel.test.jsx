// @vitest-environment jsdom
import { describe, expect, it } from 'vitest'
import { render, screen } from '@testing-library/react'
import { MediaLibraryDetailPanel } from './MediaLibraryDetailPanel'

describe('MediaLibraryDetailPanel', () => {
  it('shows the selected media metadata without an upload action', () => {
    render(
      <MediaLibraryDetailPanel
        media={{
          id: 'media-1',
          mediaType: 'IMAGE',
          status: 'READY',
          contentType: 'image/png',
          originalFileName: 'lesson.png',
          sizeBytes: 1024,
          createdAtUtc: '2026-08-22T00:00:00Z',
          contentUrl: '/media/api/media/media-1/content',
        }}
      />,
    )

    expect(screen.getByText('lesson.png')).not.toBeNull()
    expect(screen.getByText('Đã sẵn sàng')).not.toBeNull()
    expect(screen.queryByRole('button', { name: /upload/i })).toBeNull()
  })
})
