export function getDropdownPanelPlacementClass(placement = 'bottom') {
  return placement === 'top' ? 'bottom-full mb-1' : 'top-full mt-1'
}
