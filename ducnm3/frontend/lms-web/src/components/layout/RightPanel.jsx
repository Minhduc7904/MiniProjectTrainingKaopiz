import { useEffect } from 'react'
import { X } from 'lucide-react'
import { Button } from '@/components/ui/Button'
import { Icon } from '@/components/ui/Icon'
import { ui } from '@/theme'

export function RightPanel({ open, title, description, children, footer, onClose }) {
  useEffect(() => {
    if (!open) return undefined
    const handleKeyDown = (event) => {
      if (event.key === 'Escape') onClose()
    }
    document.addEventListener('keydown', handleKeyDown)
    return () => document.removeEventListener('keydown', handleKeyDown)
  }, [onClose, open])

  if (!open) return null

  return (
    <div className="fixed inset-0 z-50 flex justify-end" role="dialog" aria-modal="true" aria-label={title}>
      <button type="button" aria-label="Đóng panel" className="absolute inset-0 cursor-default bg-black/30" onClick={onClose} />
      <aside className={`relative flex h-full w-[min(440px,100vw)] flex-col shadow-2xl ${ui.page}`}>
        <header className={`flex shrink-0 items-start justify-between gap-4 px-5 py-4 ${ui.hairlineB}`}>
          <div className="min-w-0"><h2 className={`font-display text-[20px] font-semibold ${ui.title}`}>{title}</h2>{description ? <p className={`mt-1 text-[13px] ${ui.body}`}>{description}</p> : null}</div>
          <Button type="button" variant="ghost" size="icon" aria-label="Đóng" onClick={onClose}><Icon icon={X} /></Button>
        </header>
        <div className="min-h-0 flex-1 overflow-y-auto px-5 py-5">{children}</div>
        {footer ? <footer className={`shrink-0 px-5 py-4 ${ui.hairlineT}`}>{footer}</footer> : null}
      </aside>
    </div>
  )
}
