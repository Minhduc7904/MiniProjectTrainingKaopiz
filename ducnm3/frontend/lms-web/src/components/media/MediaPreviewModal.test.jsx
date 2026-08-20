// @vitest-environment jsdom
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'
import { cleanup, fireEvent, render, screen } from '@testing-library/react'

import { MediaPreviewModal } from '@/components/media/MediaPreviewModal'

beforeEach(() => {
  HTMLDialogElement.prototype.showModal = vi.fn()
  HTMLDialogElement.prototype.close = vi.fn()
})

afterEach(cleanup)

describe('MediaPreviewModal', () => {
  it('renders native video controls from contentUrl and closes on request', () => {
    const onClose = vi.fn()
    render(<MediaPreviewModal open media={{ mediaType: 'VIDEO', contentUrl: '/media/video-1/content', originalFileName: 'lesson.mp4', contentType: 'video/mp4' }} onClose={onClose} />)

    const video = document.querySelector('video')
    expect(video).not.toBeNull()
    expect(video?.hasAttribute('controls')).toBe(true)
    expect(video?.getAttribute('preload')).toBe('metadata')
    expect(document.querySelector('source')?.getAttribute('src')).toBe('http://localhost:5100/media/video-1/content')

    fireEvent.click(screen.getByRole('button', { name: 'Đóng xem trước media', hidden: true }))
    expect(onClose).toHaveBeenCalledOnce()
  })

  it('embeds a PDF through the browser viewer', () => {
    render(<MediaPreviewModal open media={{ mediaType: 'DOCUMENT', contentUrl: '/media/document-1/content', originalFileName: 'lesson.pdf', contentType: 'application/pdf' }} onClose={vi.fn()} />)

    const iframe = document.querySelector('iframe')
    expect(iframe?.getAttribute('src')).toBe('http://localhost:5100/media/document-1/content')
    expect(iframe?.getAttribute('title')).toBe('Xem trước lesson.pdf')
  })

  it('offers a download for a file without a preview renderer', () => {
    render(<MediaPreviewModal open media={{ mediaType: 'OTHER', contentUrl: '/media/file-1/content', originalFileName: 'archive.zip', contentType: 'application/zip' }} onClose={vi.fn()} />)

    const download = screen.getByRole('link', { name: 'Tải file', hidden: true })
    expect(download.getAttribute('href')).toBe('http://localhost:5100/media/file-1/content')
    expect(download.getAttribute('download')).toBe('archive.zip')
  })
})
