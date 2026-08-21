// @vitest-environment jsdom
import { afterEach, describe, expect, it } from 'vitest'
import { cleanup, render, screen } from '@testing-library/react'

import { LessonTrail } from '@/components/ui/student/LessonTrail'

afterEach(cleanup)

describe('LessonTrail', () => {
  it('announces lesson state with text as well as colour', () => {
    render(<LessonTrail items={[{ title: 'Khởi động', state: 'completed' }]} />)

    expect(screen.getByText('Khởi động')).not.toBeNull()
    expect(screen.getByText('Đã hoàn thành')).not.toBeNull()
  })
})
