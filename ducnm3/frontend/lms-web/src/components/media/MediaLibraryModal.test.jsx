// @vitest-environment jsdom
import { beforeEach, describe, expect, it, vi } from 'vitest'
import { cleanup, fireEvent, render, screen } from '@testing-library/react'
import { MediaLibraryModal } from './MediaLibraryModal'

const mediaLibraryHook = vi.hoisted(() => ({ useMediaLibrary: vi.fn() }))

vi.mock('@/hooks/media/useDirectMediaUpload', () => ({
  useDirectMediaUpload: () => ({ loading: false, reset: vi.fn(), submit: vi.fn(), error: null }),
}))

vi.mock('@/hooks/media/useMediaLibrary', () => ({
  useMediaLibrary: mediaLibraryHook.useMediaLibrary,
}))

describe('MediaLibraryModal', () => {
  beforeEach(() => {
    cleanup()
    mediaLibraryHook.useMediaLibrary.mockReturnValue({
      data: [], loading: false, error: null, hasMore: false, loadMore: vi.fn(),
    })
    mediaLibraryHook.useMediaLibrary.mockClear()
    HTMLDialogElement.prototype.showModal = vi.fn()
    HTMLDialogElement.prototype.close = vi.fn()
  })

  it('opens the direct upload pane when requested by a lesson documents tab', () => {
    render(<MediaLibraryModal open initialTab="upload" onClose={vi.fn()} onConfirm={vi.fn()} />)

    expect(screen.getByText('Upload direct')).not.toBeNull()
    expect(screen.queryByText('Loại media')).toBeNull()
  })

  it('filters the library by status from the sidebar dropdown', () => {
    render(<MediaLibraryModal open onClose={vi.fn()} onConfirm={vi.fn()} />)

    fireEvent.click(screen.getByRole('button', { name: 'Trạng thái', hidden: true }))

    fireEvent.click(screen.getByRole('option', { name: 'Đã sẵn sàng', hidden: true }))

    expect(mediaLibraryHook.useMediaLibrary).toHaveBeenLastCalledWith('', 'READY', true)
  })
})
