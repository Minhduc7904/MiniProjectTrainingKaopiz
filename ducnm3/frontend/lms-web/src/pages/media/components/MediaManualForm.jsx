import { ApiField } from '@/components/ui/admin/ApiField'
import { Button } from '@/components/ui/admin/Button'
import { FileInput, TextInput } from '@/components/ui/admin/Field'
import { HTTP_CONTENT_TYPES, HTTP_STATUS } from '@/constants/http'
import { POST_MEDIA_INPUT_FIELDS } from '@/constants/inputs/postMedia'
import { UI_LABELS } from '@/constants/ui'
import { adminUi } from '@/theme/admin'

export function MediaManualForm({
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
      <div className="min-h-0 flex-1 overflow-y-auto px-5 py-4">
        <p className={`mb-3 text-[13px] ${adminUi.body}`}>
          {HTTP_CONTENT_TYPES.multipart}. Gõ sai allowlist để xem{' '}
          {HTTP_STATUS.badRequest}/{HTTP_STATUS.unsupportedMediaType}.
        </p>
        <div className="flex flex-col gap-3">
          {POST_MEDIA_INPUT_FIELDS.map((field) => (
            <ApiField key={field.key} field={field}>
              {field.type === 'file' ? (
                <FileInput
                  key={`manual-file-${fileKey}`}
                  id={`manual-${field.key}`}
                  name={field.key}
                  disabled={loading}
                  fileName={file?.name}
                  onChange={(event) =>
                    onFileChange(event.target.files?.[0] ?? null)
                  }
                />
              ) : (
                <TextInput
                  id={`manual-${field.key}`}
                  name={field.key}
                  value={query[field.key] ?? ''}
                  disabled={loading}
                  placeholder={
                    field.allowlist
                      ? field.allowlist.join(' | ')
                      : String(field.defaultValue ?? '')
                  }
                  onChange={(event) =>
                    onQueryChange({
                      ...query,
                      [field.key]: event.target.value,
                    })
                  }
                />
              )}
            </ApiField>
          ))}
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
