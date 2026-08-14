import { ClipboardList } from 'lucide-react'
import { Icon } from '@/components/ui/Icon'
import { ICON } from '@/constants/icons'
import { ui } from '@/theme'

export function EmptyState({ title, description }) {
  return (
    <div className={`${ui.rail} rounded-lg ${ui.card} px-6 py-10 text-center`}>
      <Icon icon={ClipboardList} size={ICON.size.lg} className={ui.emptyIcon} />
      <p className={`mt-3 font-display text-[18px] font-semibold ${ui.title}`}>
        {title}
      </p>
      {description ? (
        <p className={`mt-2 text-[14px] ${ui.body}`}>{description}</p>
      ) : null}
    </div>
  )
}
