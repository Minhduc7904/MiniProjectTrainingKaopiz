import { useRef } from 'react'
import { ImagePlus } from 'lucide-react'
import { Icon } from '@/components/ui/admin/Icon'
import { adminUi } from '@/theme/admin'

/**
 * Ô soạn Markdown gọn cho mô tả/câu hỏi/lựa chọn. Chỉ có một thư viện media dùng chung ở trang cha (`onPickMedia`)
 * nên không phải mount hàng chục modal khi quiz có nhiều lựa chọn.
 */
export function QuizMarkdownField({ id, label, value, onChange, rows = 3, error, disabled = false, onPickMedia, placeholder = 'Markdown: **đậm**, $x^2$, ![ảnh](...)' }) {
  const ref = useRef(null)

  return (
    <div>
      <div className="flex items-center justify-between gap-2">
        <label htmlFor={id} className={`text-[12px] font-medium ${adminUi.body}`}>{label}</label>
        <button
          type="button"
          disabled={disabled}
          aria-label={`Chèn ảnh vào ${label.toLowerCase()}`}
          onClick={() => onPickMedia({ element: ref.current, value, onChange })}
          className={`inline-flex h-7 cursor-pointer items-center gap-1 rounded-md px-2 text-[12px] ${adminUi.body} hover:underline disabled:cursor-not-allowed disabled:opacity-50`}
        >
          <Icon icon={ImagePlus} size={14} />Ảnh
        </button>
      </div>
      <textarea
        id={id}
        ref={ref}
        rows={rows}
        value={value}
        disabled={disabled}
        placeholder={placeholder}
        aria-invalid={Boolean(error)}
        onChange={(event) => onChange(event.target.value)}
        className={`${adminUi.control} mt-1 resize-y py-2 ${error ? adminUi.controlInvalid : ''}`}
      />
      {error ? <p role="alert" className={`mt-1 text-[12px] ${adminUi.dangerText}`}>{error}</p> : null}
    </div>
  )
}
