import { CircleAlert, CircleCheck, LoaderCircle } from 'lucide-react'
import { useCallback, useState } from 'react'
import { Icon } from '@/components/ui/admin/Icon'
import { TOAST_HOVER_SCALE, TOAST_PHASE, TOAST_PHASE_LABELS } from '@/constants/toast'
import { useToastProgress } from '@/hooks/toasts/useToastProgress'
import { adminUi } from '@/theme/admin'

const railByPhase = {
  [TOAST_PHASE.pending]: adminUi.toastPendingRail,
  [TOAST_PHASE.success]: adminUi.toastSuccessRail,
  [TOAST_PHASE.error]: adminUi.toastErrorRail,
}

const barByPhase = {
  [TOAST_PHASE.pending]: adminUi.toastBarPending,
  [TOAST_PHASE.success]: adminUi.toastBarSuccess,
  [TOAST_PHASE.error]: adminUi.toastBarError,
}

const iconByPhase = {
  [TOAST_PHASE.pending]: LoaderCircle,
  [TOAST_PHASE.success]: CircleCheck,
  [TOAST_PHASE.error]: CircleAlert,
}

const iconToneByPhase = {
  [TOAST_PHASE.pending]: adminUi.toastIconPending,
  [TOAST_PHASE.success]: adminUi.toastIconSuccess,
  [TOAST_PHASE.error]: adminUi.toastIconError,
}

function formatStatus(httpStatus) {
  return httpStatus == null ? '—' : String(httpStatus)
}

function formatCode(code) {
  return code || '—'
}

export function ApiToast({ toast, onDismiss }) {
  const [paused, setPaused] = useState(false)

  const handleElapsed = useCallback(() => {
    if (toast.phase !== TOAST_PHASE.pending) {
      onDismiss(toast.id)
    }
  }, [onDismiss, toast.id, toast.phase])

  const { progress } = useToastProgress({
    id: toast.id,
    phase: toast.phase,
    durationMs: toast.durationMs,
    paused,
    onElapsed: handleElapsed,
  })

  const remaining = Math.max(0, 1 - progress)

  return (
    <article
      onMouseEnter={() => setPaused(true)}
      onMouseLeave={() => setPaused(false)}
      className={[
        'w-[360px] origin-top-right overflow-hidden rounded-lg',
        adminUi.toastFrame,
        'transition-transform duration-150 ease-[cubic-bezier(0.23,1,0.32,1)]',
        railByPhase[toast.phase],
        paused ? 'z-10' : 'z-0',
      ].join(' ')}
      style={{
        transform: paused ? `scale(${TOAST_HOVER_SCALE})` : 'scale(1)',
      }}
    >
      <div className="px-4 py-3">
        <p
          className={`flex items-center gap-2 font-display text-[11px] font-medium tracking-[0.18em] uppercase ${adminUi.eyebrow}`}
        >
          <Icon
            icon={iconByPhase[toast.phase]}
            className={iconToneByPhase[toast.phase]}
          />
          {TOAST_PHASE_LABELS[toast.phase]}
        </p>
        <p className={`mt-1 text-[14px] font-medium ${adminUi.title}`}>{toast.message}</p>
        <dl
          className={`mt-2 grid grid-cols-[auto_1fr] gap-x-3 gap-y-1 text-[12px] tabular-nums ${adminUi.body}`}
        >
          <dt>Status</dt>
          <dd>{formatStatus(toast.httpStatus)}</dd>
          <dt>Code</dt>
          <dd>{formatCode(toast.code)}</dd>
          {toast.method && toast.path ? (
            <>
              <dt>API</dt>
              <dd>
                {toast.method} {toast.path}
              </dd>
            </>
          ) : null}
        </dl>
      </div>
      <div className={`h-0.5 ${adminUi.toastBarTrack}`}>
        <div
          className={['h-full origin-left', barByPhase[toast.phase]].join(' ')}
          style={{
            transform: `scaleX(${remaining})`,
          }}
        />
      </div>
    </article>
  )
}
