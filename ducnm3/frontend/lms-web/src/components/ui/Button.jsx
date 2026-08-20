import { ui } from '@/theme'

const variants = {
  primary: ui.buttonPrimary,
  ghost: ui.buttonGhost,
}

const sizes = {
  default: 'h-9 px-3.5',
  sm: 'h-8 px-3',
  icon: 'h-9 w-9 px-0',
}

export function Button({
  children,
  type = 'button',
  variant = 'primary',
  size = 'default',
  disabled = false,
  onClick,
  form,
  'aria-label': ariaLabel,
}) {
  return (
    <button
      type={type}
      disabled={disabled}
      onClick={onClick}
      form={form}
      aria-label={ariaLabel}
      className={[
        'inline-flex cursor-pointer items-center justify-center gap-1.5 rounded-md text-[14px] font-medium',
        'disabled:cursor-not-allowed',
        sizes[size],
        variants[variant],
      ].join(' ')}
    >
      {children}
    </button>
  )
}
