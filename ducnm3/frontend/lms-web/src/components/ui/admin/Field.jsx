import { controlClassName } from '@/components/ui/admin/controlStyles'
import { adminUi } from '@/theme/admin'

export function FieldLabel({ htmlFor, children, hint }) {
  return (
    <div className="flex flex-col gap-1">
      <label
        htmlFor={htmlFor}
        className={`cursor-pointer font-display text-[11px] font-medium tracking-[0.18em] uppercase ${adminUi.eyebrow}`}
      >
        {children}
      </label>
      {hint ? <p className={`text-[12px] ${adminUi.body}`}>{hint}</p> : null}
    </div>
  )
}

export function TextInput({
  id,
  name,
  type = 'text',
  value,
  placeholder,
  disabled = false,
  invalid = false,
  onChange,
  onBlur,
}) {
  return (
    <input
      id={id}
      name={name}
      type={type}
      value={value}
      placeholder={placeholder}
      disabled={disabled}
      aria-invalid={invalid}
      onChange={onChange}
      onBlur={onBlur}
      className={[controlClassName, invalid ? adminUi.controlInvalid : ''].join(' ')}
    />
  )
}

export function FileInput({
  id,
  name,
  accept,
  disabled = false,
  invalid = false,
  fileName,
  onChange,
}) {
  return (
    <div className="flex flex-col gap-1">
      <input
        id={id}
        name={name}
        type="file"
        accept={accept}
        disabled={disabled}
        aria-invalid={invalid}
        onChange={onChange}
        className={[
          controlClassName,
          'cursor-pointer py-1.5',
          invalid ? adminUi.controlInvalid : '',
        ].join(' ')}
      />
      {fileName ? (
        <p className={`truncate text-[12px] ${adminUi.caption}`}>{fileName}</p>
      ) : null}
    </div>
  )
}
