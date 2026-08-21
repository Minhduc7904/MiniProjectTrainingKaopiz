import { studentUi } from '@/theme/student'

const variants = {
  primary: studentUi.buttonPrimary,
  bookmark: studentUi.buttonBookmark,
  quiet: studentUi.buttonQuiet,
}

export function StudentButton({ children, type = 'button', variant = 'primary', disabled = false, onClick, className = '' }) {
  return (
    <button
      className={`student-press inline-flex min-h-11 cursor-pointer items-center justify-center gap-2 rounded-2xl px-4 text-sm font-semibold outline-none focus-visible:ring-4 disabled:cursor-not-allowed ${variants[variant]} ${className}`}
      disabled={disabled}
      onClick={onClick}
      type={type}
    >
      {children}
    </button>
  )
}
