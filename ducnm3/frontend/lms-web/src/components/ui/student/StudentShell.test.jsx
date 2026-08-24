// @vitest-environment jsdom
import { render, screen } from '@testing-library/react'
import { describe, expect, it } from 'vitest'
import { StudentShell } from './StudentShell'

describe('StudentShell', () => {
  it('enables an internal vertical scroll container when requested', () => {
    render(<StudentShell scrollable><div>Nội dung dài</div></StudentShell>)

    expect(screen.getByRole('main').className).toContain('overflow-y-auto')
  })
})
