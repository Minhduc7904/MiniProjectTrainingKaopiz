import { UploadCloud } from 'lucide-react'
import { ApiField } from '@/components/ui/ApiField'
import { Button } from '@/components/ui/Button'
import { Dropdown } from '@/components/ui/Dropdown'
import { FieldLabel, FileInput, TextInput } from '@/components/ui/Field'
import { Icon } from '@/components/ui/Icon'
import { POST_MEDIA_DIRECT_INPUT_FIELDS } from '@/constants/inputs/postMediaDirect'
import {
  ACTOR_TYPE_LABELS,
  ACTOR_TYPES,
  MEDIA_TYPE_LABELS,
  MEDIA_TYPES,
  POST_MEDIA_FIELDS,
} from '@/constants/media'
import { MEDIA_COPY } from '@/constants/mediaCopy'
import { ui } from '@/theme'

const MEDIA_TYPE_OPTIONS = Object.values(MEDIA_TYPES).map((value) => ({
  value,
  label: MEDIA_TYPE_LABELS[value],
}))
const ACTOR_TYPE_OPTIONS = Object.values(ACTOR_TYPES).map((value) => ({
  value,
  label: ACTOR_TYPE_LABELS[value],
}))
const ACCEPT_BY_MEDIA_TYPE = {
  [MEDIA_TYPES.image]: 'image/*',
  [MEDIA_TYPES.video]: 'video/*',
  [MEDIA_TYPES.document]: '.pdf,.doc,.docx,.xls,.xlsx,.ppt,.pptx,.rtf,.csv,.md,.txt',
  [MEDIA_TYPES.audio]: 'audio/*',
}

export function DirectUploadForm({
  query,
  file,
  fileKey,
  loading,
  manual = false,
  onQueryChange,
  onFileChange,
  onSubmit,
}) {
  const fileControl = (
    <FileInput
      key={`${manual ? 'manual' : 'guided'}-direct-file-${fileKey}`}
      id={`${manual ? 'manual' : 'guided'}-direct-file`}
      name={POST_MEDIA_FIELDS.file}
      accept={ACCEPT_BY_MEDIA_TYPE[query[POST_MEDIA_FIELDS.mediaType]]}
      disabled={loading}
      fileName={file?.name}
      onChange={(event) => onFileChange(event.target.files?.[0] ?? null)}
    />
  )

  return (
    <form
      className="flex h-full min-h-0 flex-col"
      onSubmit={(event) => {
        event.preventDefault()
        onSubmit()
      }}
    >
      <div className="flex min-h-0 flex-1 flex-col gap-3 overflow-y-auto px-5 py-4">
        {manual ? (
          POST_MEDIA_DIRECT_INPUT_FIELDS.map((field) => (
            <ApiField key={field.key} field={field}>
              {field.type === 'file' ? fileControl : (
                <TextInput
                  id={`manual-direct-${field.key}`}
                  name={field.key}
                  value={query[field.key] ?? ''}
                  disabled={loading}
                  placeholder={field.allowlist?.join(' | ') ?? String(field.defaultValue ?? '')}
                  onChange={(event) => onQueryChange({ ...query, [field.key]: event.target.value })}
                />
              )}
            </ApiField>
          ))
        ) : (
          <>
            <div className="flex flex-col gap-1">
              <FieldLabel htmlFor="guided-direct-file" hint="File được hash theo từng chunk; không được lưu vào Redux hoặc browser storage.">
                {MEDIA_COPY.file}
              </FieldLabel>
              {fileControl}
            </div>
            <Dropdown
              label={MEDIA_COPY.mediaType}
              className="w-full min-w-0"
              value={query[POST_MEDIA_FIELDS.mediaType]}
              options={MEDIA_TYPE_OPTIONS}
              disabled={loading}
              onChange={(value) => onQueryChange({ ...query, [POST_MEDIA_FIELDS.mediaType]: value })}
            />
            <Dropdown
              label={MEDIA_COPY.uploadedByType}
              className="w-full min-w-0"
              value={query[POST_MEDIA_FIELDS.uploadedByType]}
              options={ACTOR_TYPE_OPTIONS}
              disabled={loading}
              onChange={(value) => onQueryChange({ ...query, [POST_MEDIA_FIELDS.uploadedByType]: value })}
            />
            <div className="flex flex-col gap-1">
              <FieldLabel htmlFor="guided-direct-uploaded-by">{MEDIA_COPY.uploadedBy}</FieldLabel>
              <TextInput
                id="guided-direct-uploaded-by"
                name={POST_MEDIA_FIELDS.uploadedBy}
                value={query[POST_MEDIA_FIELDS.uploadedBy] ?? ''}
                disabled={loading}
                placeholder="UUID"
                onChange={(event) => onQueryChange({ ...query, [POST_MEDIA_FIELDS.uploadedBy]: event.target.value })}
              />
            </div>
          </>
        )}
      </div>
      <div className={`shrink-0 px-5 py-3 ${ui.hairlineT}`}>
        <Button type="submit" disabled={loading || !file}>
          <Icon icon={UploadCloud} />
          {MEDIA_COPY.directUploadAction}
        </Button>
      </div>
    </form>
  )
}
