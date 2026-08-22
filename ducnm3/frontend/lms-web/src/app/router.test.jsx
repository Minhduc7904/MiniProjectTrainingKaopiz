// @vitest-environment jsdom
import { describe, expect, it, vi } from 'vitest'
import { render, screen } from '@testing-library/react'
import { MemoryRouter } from 'react-router-dom'

vi.mock('@/pages/student-auth/StudentAuthPages', () => {
  const Placeholder = () => null

  return {
    StudentCourseDetailPage: Placeholder,
    StudentCourseLearnPage: Placeholder,
    StudentCoursesPage: Placeholder,
    StudentHomePage: Placeholder,
    StudentLoadingPage: Placeholder,
    StudentLoginPage: () => <h1>Đăng nhập Student</h1>,
    StudentLogoutPage: Placeholder,
    StudentProfilePage: Placeholder,
    StudentRegisterPage: Placeholder,
  }
})

import { AppRouter } from './router'
import { APP_ROUTES } from '@/constants/appRoutes'

describe('AppRouter', () => {
  it('redirects the root route to Student login', () => {
    render(
      <MemoryRouter initialEntries={[APP_ROUTES.home]}>
        <AppRouter />
      </MemoryRouter>,
    )

    expect(screen.getByRole('heading', { name: 'Đăng nhập Student' })).not.toBeNull()
  })
})
