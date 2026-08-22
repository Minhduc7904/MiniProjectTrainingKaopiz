// @vitest-environment jsdom
import { describe, expect, it, vi } from 'vitest'
import { fireEvent, render, screen } from '@testing-library/react'
import { LessonDetailTabs } from './LessonDetailTabs'

vi.mock('@/components/markdown/RenderedMarkdown', () => ({
  RenderedMarkdown: ({ html }) => <div data-testid="lesson-content">{html}</div>,
}))

vi.mock('@/pages/media/components/MediaImagePreview', () => ({
  MediaImagePreview: ({ alt }) => <img alt={alt} />,
}))

const lesson = {
  id: 'lesson-1',
  title: 'Đạo hàm',
  contentHtml: '<p>Nội dung lesson</p>',
  attachments: [{
    usageId: 'usage-1',
    mediaType: 'DOCUMENT',
    contentType: 'application/pdf',
    originalFileName: 'bai-tap.pdf',
    contentUrl: 'https://example.test/bai-tap.pdf',
    thumbnailUrl: null,
  }],
}

describe('LessonDetailTabs', () => {
  it('shows lesson content first and keeps media actions in the documents tab', () => {
    const onOpenLibrary = vi.fn()
    const onOpenUpload = vi.fn()
    const onPreviewMedia = vi.fn()
    const onRemoveMedia = vi.fn()

    render(
      <LessonDetailTabs
        lesson={lesson}
        lessonNumber={1}
        mediaSaving={false}
        onEdit={vi.fn()}
        onDelete={vi.fn()}
        onOpenLibrary={onOpenLibrary}
        onOpenUpload={onOpenUpload}
        onPreviewMedia={onPreviewMedia}
        onRemoveMedia={onRemoveMedia}
      />,
    )

    expect(screen.getByRole('tab', { name: 'Nội dung' }).getAttribute('aria-selected')).toBe('true')
    expect(screen.getByTestId('lesson-content').textContent).toBe('<p>Nội dung lesson</p>')
    expect(screen.queryByText('bai-tap.pdf')).toBeNull()

    fireEvent.click(screen.getByRole('tab', { name: 'Tài liệu (1)' }))

    expect(screen.getByText('bai-tap.pdf')).not.toBeNull()
    fireEvent.click(screen.getByRole('button', { name: 'Chọn từ thư viện' }))
    fireEvent.click(screen.getByRole('button', { name: 'Upload tài liệu' }))
    fireEvent.click(screen.getByRole('button', { name: 'Gỡ bai-tap.pdf' }))

    expect(onOpenLibrary).toHaveBeenCalledOnce()
    expect(onOpenUpload).toHaveBeenCalledOnce()
    expect(onRemoveMedia).toHaveBeenCalledWith('usage-1')
    expect(onPreviewMedia).not.toHaveBeenCalled()
  })
})
