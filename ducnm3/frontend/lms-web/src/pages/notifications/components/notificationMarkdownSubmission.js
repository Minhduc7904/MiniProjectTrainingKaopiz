export function preventParentBatchSubmit(event) {
  event.preventDefault()
  event.stopPropagation()
}
