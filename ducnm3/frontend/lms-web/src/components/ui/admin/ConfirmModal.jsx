import { useEffect, useRef } from 'react'
import { AlertTriangle, Info, Trash2 } from 'lucide-react'
import { Button } from '@/components/ui/admin/Button'
import { Icon } from '@/components/ui/admin/Icon'
import { adminUi } from '@/theme/admin'

const modes = {
  info: { icon: Info, tone: adminUi.confirmInfo },
  warning: { icon: AlertTriangle, tone: adminUi.confirmWarning },
  delete: { icon: Trash2, tone: adminUi.confirmDelete },
}

export function ConfirmModal({ open, mode = 'info', title, description, confirmLabel = 'Xác nhận', busy = false, onConfirm, onClose }) {
  const dialogRef = useRef(null)
  const current = modes[mode] ?? modes.info
  useEffect(() => {
    const dialog = dialogRef.current
    if (!dialog) return
    if (open && !dialog.open) dialog.showModal()
    if (!open && dialog.open) dialog.close()
  }, [open])
  return <dialog ref={dialogRef} className={adminUi.modal} onCancel={(event) => { event.preventDefault(); if (!busy) onClose() }}>
    <div className="p-5">
      <div className="flex gap-3">
        <span className={`flex h-10 w-10 shrink-0 items-center justify-center rounded-full ${current.tone}`}><Icon icon={current.icon} size={19} /></span>
        <div><h2 className={`text-[17px] font-semibold ${adminUi.title}`}>{title}</h2><p className={`mt-1 text-[14px] leading-6 ${adminUi.body}`}>{description}</p></div>
      </div>
    </div>
    <div className={`flex justify-end gap-2 px-5 py-3 ${adminUi.hairlineT}`}>
      <Button variant="ghost" disabled={busy} onClick={onClose}>Hủy</Button>
      <Button disabled={busy} onClick={onConfirm} variant={mode === 'delete' ? 'danger' : 'primary'}>{busy ? 'Đang xử lý...' : confirmLabel}</Button>
    </div>
  </dialog>
}
