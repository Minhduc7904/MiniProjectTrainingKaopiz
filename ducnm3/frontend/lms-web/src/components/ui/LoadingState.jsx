import { Spinner } from '@/components/ui/Spinner'
import { UI_LABELS } from '@/constants/ui'
import { ui } from '@/theme'

export function LoadingState({ label = UI_LABELS.loading }) {
  return (
    <div className={`flex items-center gap-2 text-[13px] ${ui.body}`}>
      <Spinner />
      <span>{label}</span>
    </div>
  )
}
