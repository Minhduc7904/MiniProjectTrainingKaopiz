import { describe, expect, it } from 'vitest'
import { resolveMediaContentUrl } from '@/components/media/mediaContentUrl'

describe('resolveMediaContentUrl', () => {
  it('resolves a Media Service relative content URL through the configured API origin', () => {
    expect(resolveMediaContentUrl('/media/api/media/video-1/content'))
      .toBe('http://localhost:5100/media/api/media/video-1/content')
  })

  it('does not change an absolute content URL', () => {
    expect(resolveMediaContentUrl('https://cdn.example.test/video.mp4'))
      .toBe('https://cdn.example.test/video.mp4')
  })
})
