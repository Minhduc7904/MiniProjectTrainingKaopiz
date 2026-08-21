import { studentUi } from '@/theme/student'

export function StudentInput({ id, label, hint, error, type = 'text', value, onChange, placeholder, required = false, disabled = false, name = id, autoComplete, inputMode, spellCheck }) {
  const describedBy = [hint ? `${id}-hint` : null, error ? `${id}-error` : null].filter(Boolean).join(' ') || undefined

  return (
    <div className="grid gap-2">
      <label className={studentUi.label} htmlFor={id}>{label}</label>
      {hint ? <p className={studentUi.caption} id={`${id}-hint`}>{hint}</p> : null}
      <input
        aria-describedby={describedBy}
        aria-invalid={Boolean(error)}
        className={`${studentUi.control} ${error ? studentUi.controlInvalid : ''}`}
        disabled={disabled}
        autoComplete={autoComplete}
        id={id}
        inputMode={inputMode}
        name={name}
        onChange={onChange}
        placeholder={placeholder}
        required={required}
        spellCheck={spellCheck}
        type={type}
        value={value}
      />
      {error ? <p className={studentUi.error} id={`${id}-error`}>{error}</p> : null}
    </div>
  )
}
