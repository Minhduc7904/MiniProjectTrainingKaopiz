import { UploadCloud } from 'lucide-react'
import { ApiField } from '@/components/ui/admin/ApiField'
import { Button } from '@/components/ui/admin/Button'
import { FieldLabel, FileInput, TextInput } from '@/components/ui/admin/Field'
import { Icon } from '@/components/ui/admin/Icon'
import { POST_MEDIA_DIRECT_INPUT_FIELDS } from '@/constants/inputs/postMediaDirect'
import {
  POST_MEDIA_FIELDS,
} from '@/constants/media'
import { MEDIA_COPY } from '@/constants/mediaCopy'
import { adminUi } from '@/theme/admin'

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
          </>
        )}
      </div>
      <div className={`shrink-0 px-5 py-3 ${adminUi.hairlineT}`}>
        <Button type="submit" disabled={loading || !file}>
          <Icon icon={UploadCloud} />
          {MEDIA_COPY.directUploadAction}
        </Button>
      </div>
    </form>
  )
}
