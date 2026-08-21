import { Check, ChevronDown } from 'lucide-react'
import { useCallback, useId, useMemo, useRef, useState } from 'react'
import { FieldLabel } from '@/components/ui/admin/Field'
import { Icon } from '@/components/ui/admin/Icon'
import { controlClassName } from '@/components/ui/admin/controlStyles'
import { KEYBOARD } from '@/constants/keyboard'
import { UI_LABELS } from '@/constants/ui'
import { adminUi } from '@/theme/admin'
import { useClickOutside } from '@/hooks/ui/useClickOutside'
import { getDropdownPanelPlacementClass } from '@/components/ui/admin/dropdownPlacement'

export function Dropdown({
  id,
  label,
  value,
  options,
  disabled = false,
  placement = 'bottom',
  placeholder = UI_LABELS.select,
  className = '',
  triggerClassName = '',
  onChange,
}) {
  const generatedId = useId()
  const fieldId = id ?? generatedId
  const rootRef = useRef(null)
  const [open, setOpen] = useState(false)
  const [highlightIndex, setHighlightIndex] = useState(-1)

  const selected = useMemo(
    () => options.find((option) => option.value === value),
    [options, value],
  )

  const close = useCallback(() => {
    setOpen(false)
    setHighlightIndex(-1)
  }, [])

  useClickOutside(rootRef, close, open)

  const selectOption = (option) => {
    onChange(option.value)
    close()
  }

  const handleKeyDown = (event) => {
    if (disabled) {
      return
    }

    if (event.key === KEYBOARD.escape) {
      event.preventDefault()
      close()
      return
    }

    if (event.key === KEYBOARD.enter) {
      event.preventDefault()
      if (!open) {
        setOpen(true)
        return
      }

      const option = options[highlightIndex]
      if (option) {
        selectOption(option)
      }
      return
    }

    if (event.key === KEYBOARD.arrowDown || event.key === KEYBOARD.arrowUp) {
      event.preventDefault()
      if (!open) {
        setOpen(true)
        setHighlightIndex(0)
        return
      }

      const delta = event.key === KEYBOARD.arrowDown ? 1 : -1
      setHighlightIndex((current) => {
        const next = current + delta
        if (next < 0) {
          return options.length - 1
        }

        if (next >= options.length) {
          return 0
        }

        return next
      })
    }
  }

  return (
    <div
      ref={rootRef}
      className={['flex min-w-[160px] flex-col gap-1', className].join(' ')}
    >
      {label ? <FieldLabel htmlFor={fieldId}>{label}</FieldLabel> : null}
      <div className="relative">
        <button
          id={fieldId}
          type="button"
          disabled={disabled}
          aria-haspopup="listbox"
          aria-expanded={open}
          onClick={() => {
            if (disabled) {
              return
            }

            setOpen((current) => !current)
          }}
          onKeyDown={handleKeyDown}
          className={[
            controlClassName,
            triggerClassName,
            'flex cursor-pointer items-center justify-between gap-2 text-left disabled:cursor-not-allowed',
          ].join(' ')}
        >
          <span className={selected ? adminUi.dropdownValue : adminUi.dropdownPlaceholder}>
            {selected?.label ?? placeholder}
          </span>
          <Icon
            icon={ChevronDown}
            className={[adminUi.dropdownChevron, open ? 'rotate-180' : ''].join(' ')}
          />
        </button>
        {open ? (
          <ul
            role="listbox"
            className={[adminUi.dropdownPanel, getDropdownPanelPlacementClass(placement)].join(' ')}
          >
            {options.map((option, index) => {
              const active = option.value === value
              const highlighted = index === highlightIndex

              return (
                <li key={String(option.value)}>
                  <button
                    type="button"
                    role="option"
                    aria-selected={active}
                    onMouseEnter={() => setHighlightIndex(index)}
                    onClick={() => selectOption(option)}
                    className={[
                      'flex w-full cursor-pointer items-center justify-between gap-2 px-3 py-2 text-left text-[14px]',
                      highlighted || active
                        ? adminUi.dropdownOptionActive
                        : adminUi.dropdownOptionIdle,
                      active ? 'font-medium' : 'font-normal',
                    ].join(' ')}
                  >
                    <span>{option.label}</span>
                    {active ? <Icon icon={Check} className={adminUi.check} /> : null}
                  </button>
                </li>
              )
            })}
          </ul>
        ) : null}
      </div>
    </div>
  )
}
