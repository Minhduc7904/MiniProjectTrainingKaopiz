import { ICON } from '@/constants/icons'

export function Icon({
  icon: LucideIcon,
  size = ICON.size.sm,
  className,
  ...props
}) {
  return (
    <LucideIcon
      size={size}
      strokeWidth={ICON.strokeWidth}
      className={className}
      aria-hidden="true"
      {...props}
    />
  )
}
