// @vitest-environment jsdom
import { afterEach, describe, expect, it } from 'vitest'
import { cleanup, render, screen } from '@testing-library/react'

import { Button } from '@/components/ui/admin/Button'
import { adminUi } from '@/theme/admin'

afterEach(cleanup)

describe('Admin UI namespace', () => {
  it('keeps the Admin primary button bound to the Admin theme', () => {
    render(<Button>Save</Button>)

    expect(screen.getByRole('button', { name: 'Save' }).className)
      .toContain(adminUi.buttonPrimary)
  })
})
