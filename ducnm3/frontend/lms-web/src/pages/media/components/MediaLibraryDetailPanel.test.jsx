// @vitest-environment jsdom
import { afterEach, describe, expect, it, vi } from 'vitest'
import { cleanup, render, screen } from '@testing-library/react'

vi.mock('@/pages/media/components/MediaImagePreview', () => ({
  MediaImagePreview: ({ contentUrl }) => <span data-testid="media-preview-url">{contentUrl}</span>,
}))
import { MediaLibraryDetailPanel } from './MediaLibraryDetailPanel'

afterEach(cleanup)

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
    expect(screen.getByTestId('media-preview-url').textContent)
      .toBe('/media/api/media/media-1/content')
    expect(screen.queryByRole('button', { name: /upload/i })).toBeNull()
  })

  it('shows thumbnail metadata separately from the original media', () => {
    const { container } = render(
      <MediaLibraryDetailPanel
        media={{
          id: 'video-1',
          mediaType: 'VIDEO',
          status: 'READY',
          contentType: 'video/mp4',
          originalFileName: 'lesson.mp4',
          sizeBytes: 2048,
          createdAtUtc: '2026-08-22T00:00:00Z',
          contentUrl: '/media/api/media/video-1/content',
          thumbnail: {
            id: 'thumb-1',
            status: 'READY',
            contentType: 'image/webp',
            sizeBytes: 512,
            createdAtUtc: '2026-08-22T00:01:00Z',
            completedAtUtc: '2026-08-22T00:01:05Z',
            contentUrl: '/media/api/media/thumb-1/content',
          },
        }}
      />,
    )

    expect(screen.getByText('Thông tin thumbnail')).not.toBeNull()
    expect(screen.getByText('thumb-1')).not.toBeNull()
    expect(screen.getByText('image/webp')).not.toBeNull()
    expect(container.querySelector('video source')?.getAttribute('src'))
      .toBe('http://localhost:5100/media/api/media/video-1/content')
  })
})
