import { Spinner } from '@/components/ui/admin/Spinner'
import { UI_LABELS } from '@/constants/ui'
import { adminUi } from '@/theme/admin'

export function LoadingState({ label = UI_LABELS.loading }) {
  return (
    <div className={`flex items-center gap-2 text-[13px] ${adminUi.body}`}>
      <Spinner />
      <span>{label}</span>
    </div>
  )
}
