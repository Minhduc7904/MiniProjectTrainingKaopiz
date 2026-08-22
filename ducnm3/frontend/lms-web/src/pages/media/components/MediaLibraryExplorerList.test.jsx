// @vitest-environment jsdom
import { describe, expect, it, vi } from 'vitest'
import { render, screen } from '@testing-library/react'

vi.mock('@/pages/media/components/MediaImagePreview', () => ({
  MediaImagePreview: ({ contentUrl }) => <span data-testid="media-thumbnail">{contentUrl}</span>,
}))

import { MediaLibraryExplorerList } from './MediaLibraryExplorerList'

describe('MediaLibraryExplorerList', () => {
  it('uses a ready video thumbnail instead of the original video content', () => {
    render(
      <MediaLibraryExplorerList
        items={[
          {
            id: 'video-1',
            mediaType: 'VIDEO',
            status: 'READY',
            originalFileName: 'lesson.mp4',
            contentUrl: '/media/api/media/video-1/content',
            thumbnail: {
              status: 'READY',
              contentUrl: '/media/api/media/thumb-1/content',
            },
          },
        ]}
        selectedId={null}
        viewMode="grid"
        onSelect={vi.fn()}
      />,
    )

    expect(screen.getByTestId('media-thumbnail').textContent)
      .toBe('/media/api/media/thumb-1/content')
  })
})
