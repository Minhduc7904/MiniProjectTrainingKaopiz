// @vitest-environment jsdom
import { beforeEach, describe, expect, it, vi } from 'vitest'
import { render, screen } from '@testing-library/react'
import { MediaLibraryModal } from './MediaLibraryModal'

vi.mock('@/hooks/media/useDirectMediaUpload', () => ({
  useDirectMediaUpload: () => ({ loading: false, reset: vi.fn(), submit: vi.fn(), error: null }),
}))

vi.mock('@/hooks/media/useMediaLibrary', () => ({
  useMediaLibrary: () => ({ data: [], loading: false, error: null, hasMore: false, loadMore: vi.fn() }),
}))

describe('MediaLibraryModal', () => {
  beforeEach(() => {
    HTMLDialogElement.prototype.showModal = vi.fn()
    HTMLDialogElement.prototype.close = vi.fn()
  })

  it('opens the direct upload pane when requested by a lesson documents tab', () => {
    render(<MediaLibraryModal open initialTab="upload" onClose={vi.fn()} onConfirm={vi.fn()} />)

    expect(screen.getByText('Upload direct')).not.toBeNull()
    expect(screen.queryByText('Loại media')).toBeNull()
  })
})
