import { Button } from '@/components/ui/admin/Button'
import { AI_COPY } from '@/constants/ai'
import { adminUi } from '@/theme/admin'

export function AiFormShell({ title, editing, saving, submitLabel, onSubmit, onCancel, children }) {
  return (
    <form
      className={`mt-5 flex flex-col gap-3 pt-4 ${adminUi.hairlineT}`}
      onSubmit={(event) => {
        event.preventDefault()
        onSubmit()
      }}
    >
      <p className={`font-display text-[14px] font-semibold ${adminUi.title}`}>{title}</p>
      {children}
      <div className="flex gap-2">
        <Button type="submit" disabled={saving}>
          {saving ? 'Đang lưu...' : submitLabel}
        </Button>
        {editing ? (
          <Button variant="ghost" disabled={saving} onClick={onCancel}>
            {AI_COPY.cancelEdit}
          </Button>
        ) : null}
      </div>
    </form>
  )
}
