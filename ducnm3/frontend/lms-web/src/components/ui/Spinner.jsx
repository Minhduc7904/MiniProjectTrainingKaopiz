import { LoaderCircle } from 'lucide-react'
import { Icon } from '@/components/ui/Icon'
import { ICON } from '@/constants/icons'
import { ui } from '@/theme'

export function Spinner({ size = ICON.size.sm }) {
  return <Icon icon={LoaderCircle} size={size} className={ui.spinner} />
}
