// @vitest-environment jsdom
import { fireEvent, render, screen } from '@testing-library/react'
import { describe, expect, it, vi } from 'vitest'
import { StudentCourseCatalogSearch } from './StudentCourseCatalogSearch'

describe('StudentCourseCatalogSearch', () => {
  it('keeps the search query controlled and submits it explicitly', () => {
    const onChange = vi.fn()
    const onSubmit = vi.fn()
    const query = { page: 1, pageSize: 12 }
    const { rerender } = render(<StudentCourseCatalogSearch query={query} loading={false} onChange={onChange} onSubmit={onSubmit} />)

    fireEvent.change(screen.getByLabelText('Tìm khóa học'), { target: { value: 'Backend' } })
    const searchedQuery = { ...query, search: 'Backend', page: 1 }
    expect(onChange).toHaveBeenCalledWith(searchedQuery)

    rerender(<StudentCourseCatalogSearch query={searchedQuery} loading={false} onChange={onChange} onSubmit={onSubmit} />)
    fireEvent.submit(screen.getByRole('button', { name: 'Tìm kiếm khóa học' }).closest('form'))
    expect(onSubmit).toHaveBeenCalledWith(searchedQuery)
  })
})
