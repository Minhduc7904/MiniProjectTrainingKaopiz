import { Pencil, Trash2 } from 'lucide-react'
import { Button } from '@/components/ui/admin/Button'
import { Icon } from '@/components/ui/admin/Icon'
import { AI_COPY } from '@/constants/ai'

export function AiRowActions({ label, disabled = false, onEdit, onDelete }) {
  return (
    <div className="flex justify-end gap-1.5">
      <Button
        variant="ghost"
        size="sm"
        disabled={disabled}
        aria-label={`${AI_COPY.edit} ${label}`}
        onClick={onEdit}
      >
        <Icon icon={Pencil} />
        {AI_COPY.edit}
      </Button>
      <Button
        variant="danger"
        size="sm"
        disabled={disabled}
        aria-label={`${AI_COPY.remove} ${label}`}
        onClick={onDelete}
      >
        <Icon icon={Trash2} />
        {AI_COPY.remove}
      </Button>
    </div>
  )
}
