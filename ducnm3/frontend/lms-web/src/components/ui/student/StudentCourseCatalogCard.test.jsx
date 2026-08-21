// @vitest-environment jsdom
import { describe, expect, it, vi } from 'vitest'
import { fireEvent, render, screen } from '@testing-library/react'
import { StudentCourseCatalogCard } from './StudentCourseCatalogCard'

describe('StudentCourseCatalogCard', () => {
  it('exposes a registration action for an available Course', () => {
    const onEnroll = vi.fn()
    render(<StudentCourseCatalogCard course={{ courseId: '11111111-1111-1111-1111-111111111111', name: 'Backend Fundamentals', status: 'PUBLISHED', thumbnailUrl: null }} onEnroll={onEnroll} />)

    fireEvent.click(screen.getByRole('button', { name: 'Đăng kí Backend Fundamentals' }))

    expect(onEnroll).toHaveBeenCalledWith('11111111-1111-1111-1111-111111111111')
  })
})
