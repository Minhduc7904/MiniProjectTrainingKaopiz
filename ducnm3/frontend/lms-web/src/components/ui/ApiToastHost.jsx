import { ApiToast } from '@/components/ui/ApiToast'
import { useApiToasts } from '@/hooks/toasts/useApiToasts'

export function ApiToastHost() {
  const { items, dismiss } = useApiToasts()

  if (items.length === 0) {
    return null
  }

  return (
    <div className="pointer-events-none fixed top-6 right-6 z-50 flex flex-col items-end gap-2">
      {items.map((toast) => (
        <div key={toast.id} className="pointer-events-auto">
          <ApiToast toast={toast} onDismiss={dismiss} />
        </div>
      ))}
    </div>
  )
}
