export async function executeDirectUploadWorkflow({
  file,
  mediaType,
  actor,
  runtime,
  isCurrent,
  operations,
  events,
  sanitizeError = (error) => error,
}) {
  try {
    events.checksumStarted()
    const checksumOperation = operations.hashFile(
      file,
      (progress) => {
        if (isCurrent()) events.checksumProgress(progress)
      },
    )
    runtime.setChecksum(checksumOperation)
    let checksumSha256
    try {
      checksumSha256 = await checksumOperation
    } finally {
      runtime.clearChecksum(checksumOperation)
    }
    if (!isCurrent()) return false
    events.checksumCompleted(checksumSha256)
    events.preparingStarted()

    const intent = await operations.createIntent(
      {
        originalFileName: file.name,
        mediaType,
        contentType: file.type,
        sizeBytes: file.size,
        checksumSha256,
        ...actor,
      },
      { signal: runtime.signal },
    )
    if (!isCurrent()) return false
    events.uploadStarted(intent.data.mediaId, intent.meta.traceId)

    const uploadOperation = operations.uploadSigned({
      uploadUrl: intent.data.uploadUrl,
      formFields: intent.data.formFields,
      file,
      onProgress: (progress) => {
        if (isCurrent()) events.uploadProgress(progress)
      },
    })
    runtime.setUpload(uploadOperation)
    try {
      await uploadOperation
    } finally {
      runtime.clearUpload(uploadOperation)
    }
    if (!isCurrent()) return false
    events.finalizeStarted()

    const completed = await operations.complete(
      intent.data.mediaId,
      actor,
      { signal: runtime.signal },
    )
    if (!isCurrent()) return false
    events.completed(completed.data, completed.meta.traceId)
    return true
  } catch (error) {
    if (isCurrent()) events.failed(sanitizeError(error))
    return false
  }
}
