import { describe, expect, it } from 'vitest'
import { getDropdownPanelPlacementClass } from '@/components/ui/dropdownPlacement'

describe('getDropdownPanelPlacementClass', () => {
  it('positions a top placement above its trigger', () => {
    expect(getDropdownPanelPlacementClass('top')).toContain('bottom-full')
    expect(getDropdownPanelPlacementClass('top')).toContain('mb-1')
  })

  it('keeps the default placement below its trigger', () => {
    expect(getDropdownPanelPlacementClass()).toContain('top-full')
    expect(getDropdownPanelPlacementClass()).toContain('mt-1')
  })
})
