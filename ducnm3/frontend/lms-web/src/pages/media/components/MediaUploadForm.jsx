import { Button } from '@/components/ui/Button'
import { Dropdown } from '@/components/ui/Dropdown'
import { FieldLabel, FileInput, TextInput } from '@/components/ui/Field'
import {
  ACTOR_TYPE_LABELS,
  ACTOR_TYPES,
  MEDIA_TYPE_LABELS,
  MEDIA_TYPES,
  POST_MEDIA_FIELDS,
} from '@/constants/media'
import { MEDIA_COPY } from '@/constants/mediaCopy'
import { UI_LABELS } from '@/constants/ui'
import { ui } from '@/theme'

const MEDIA_TYPE_OPTIONS = Object.values(MEDIA_TYPES).map((value) => ({
  value,
  label: MEDIA_TYPE_LABELS[value],
}))

const ACTOR_TYPE_OPTIONS = Object.values(ACTOR_TYPES).map((value) => ({
  value,
  label: ACTOR_TYPE_LABELS[value],
}))

export function MediaUploadForm({
  query,
  file,
  fileKey,
  loading,
  onQueryChange,
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
        <Dropdown
          label={MEDIA_COPY.mediaType}
          className="w-full min-w-0"
          value={query[POST_MEDIA_FIELDS.mediaType] ?? ''}
          options={MEDIA_TYPE_OPTIONS}
          disabled={loading}
          onChange={(mediaType) =>
            onQueryChange({
              ...query,
              [POST_MEDIA_FIELDS.mediaType]: mediaType,
            })
          }
        />
        <Dropdown
          label={MEDIA_COPY.uploadedByType}
          className="w-full min-w-0"
          value={query[POST_MEDIA_FIELDS.uploadedByType] ?? ''}
          options={ACTOR_TYPE_OPTIONS}
          disabled={loading}
          onChange={(uploadedByType) =>
            onQueryChange({
              ...query,
              [POST_MEDIA_FIELDS.uploadedByType]: uploadedByType,
            })
          }
        />
        <div className="flex flex-col gap-1">
          <FieldLabel htmlFor="guided-uploaded-by">
            {MEDIA_COPY.uploadedBy}
          </FieldLabel>
          <TextInput
            id="guided-uploaded-by"
            name={POST_MEDIA_FIELDS.uploadedBy}
            value={query[POST_MEDIA_FIELDS.uploadedBy] ?? ''}
            disabled={loading}
            placeholder="UUID"
            onChange={(event) =>
              onQueryChange({
                ...query,
                [POST_MEDIA_FIELDS.uploadedBy]: event.target.value,
              })
            }
          />
        </div>
      </div>
      <div className={`shrink-0 px-5 py-3 ${ui.hairlineT}`}>
        <Button type="submit" disabled={loading}>
          {UI_LABELS.callApi}
        </Button>
      </div>
    </form>
  )
}
