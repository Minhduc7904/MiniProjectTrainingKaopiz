export function createDirectUploadRuntime() {
  const controller = new AbortController()
  let checksumOperation = null
  let uploadOperation = null

  function setOperation(operation, assign) {
    if (controller.signal.aborted) {
      operation?.abort?.()
      return
    }
    assign(operation)
  }

  return {
    signal: controller.signal,
    setChecksum(operation) {
      setOperation(operation, (value) => { checksumOperation = value })
    },
    clearChecksum(operation) {
      if (checksumOperation === operation) checksumOperation = null
    },
    setUpload(operation) {
      setOperation(operation, (value) => { uploadOperation = value })
    },
    clearUpload(operation) {
      if (uploadOperation === operation) uploadOperation = null
    },
    abort() {
      if (!controller.signal.aborted) controller.abort()
      checksumOperation?.abort?.()
      uploadOperation?.abort?.()
      checksumOperation = null
      uploadOperation = null
    },
  }
}
