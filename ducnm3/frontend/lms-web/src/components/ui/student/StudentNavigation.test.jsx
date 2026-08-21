// @vitest-environment jsdom
import { describe, expect, it } from 'vitest'
import { render, screen } from '@testing-library/react'
import { MemoryRouter } from 'react-router-dom'
import { StudentNavigation } from './StudentNavigation'

describe('StudentNavigation', () => {
  it('keeps Home and Khóa học as the centered Student destinations', () => {
    render(
      <MemoryRouter initialEntries={['/student/home']}>
        <StudentNavigation />
      </MemoryRouter>,
    )

    expect(screen.getByRole('link', { name: 'Home' }).getAttribute('href')).toBe('/student/home')
    expect(screen.getByRole('link', { name: 'Khóa học' }).getAttribute('href')).toBe('/student/courses')
  })
})
