import { Button } from '@/components/ui/admin/Button'
import { FieldLabel, FileInput } from '@/components/ui/admin/Field'
import { POST_MEDIA_FIELDS } from '@/constants/media'
import { MEDIA_COPY } from '@/constants/mediaCopy'
import { UI_LABELS } from '@/constants/ui'
import { adminUi } from '@/theme/admin'

export function MediaUploadForm({
  file,
  fileKey,
  loading,
  onFileChange,
  onSubmit,
}) {
  return (
    <form
      className="flex h-full min-h-0 flex-col"
      onSubmit={(event) => {
        event.preventDefault()
        onSubmit()
      }}
    >
      <div className="flex min-h-0 flex-1 flex-col gap-3 overflow-y-auto px-5 py-4">
        <div className="flex flex-col gap-1">
          <FieldLabel htmlFor="guided-file">{MEDIA_COPY.file}</FieldLabel>
          <FileInput
            key={`guided-file-${fileKey}`}
            id="guided-file"
            name={POST_MEDIA_FIELDS.file}
            disabled={loading}
            fileName={file?.name}
            onChange={(event) => onFileChange(event.target.files?.[0] ?? null)}
          />
        </div>
      </div>
      <div className={`shrink-0 px-5 py-3 ${adminUi.hairlineT}`}>
        <Button type="submit" disabled={loading}>
          {UI_LABELS.callApi}
        </Button>
      </div>
    </form>
  )
}
