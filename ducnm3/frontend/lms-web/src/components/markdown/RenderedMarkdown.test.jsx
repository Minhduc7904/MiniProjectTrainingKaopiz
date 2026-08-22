// @vitest-environment jsdom
import { describe, expect, it, vi } from 'vitest'
import { render, waitFor } from '@testing-library/react'
import katex from 'katex'
import { RenderedMarkdown } from './RenderedMarkdown'

vi.mock('katex', () => ({
  default: { render: vi.fn() },
}))

describe('RenderedMarkdown', () => {
  it('renders only backend-generated math placeholders with untrusted KaTeX options', async () => {
    const { container } = render(
      <RenderedMarkdown html={'<p>Tính <span class="course-math" data-math-display="inline" data-math-tex="f&#39;(x)"></span>.</p><p><span class="course-math" data-math-display="block" data-math-tex="\\boxed{x}"></span></p><span class="course-math">Không render</span>'} />,
    )

    await waitFor(() => expect(katex.render).toHaveBeenCalledTimes(2))

    expect(katex.render).toHaveBeenNthCalledWith(
      1,
      "f'(x)",
      container.querySelector('[data-math-display="inline"]'),
      expect.objectContaining({ displayMode: false, throwOnError: false, trust: false }),
    )
    expect(katex.render).toHaveBeenNthCalledWith(
      2,
      '\\boxed{x}',
      container.querySelector('[data-math-display="block"]'),
      expect.objectContaining({ displayMode: true, throwOnError: false, trust: false }),
    )
  })
})
