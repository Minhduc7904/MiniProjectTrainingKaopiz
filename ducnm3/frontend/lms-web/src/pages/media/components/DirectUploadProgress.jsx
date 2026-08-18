import { BadgeCheck, Hash, UploadCloud } from 'lucide-react'
import { Icon } from '@/components/ui/Icon'
import { MEDIA_COPY } from '@/constants/mediaCopy'
import {
  DIRECT_UPLOAD_PHASES,
  DIRECT_UPLOAD_PHASE_LABELS,
} from '@/features/media/directUploadSlice'
import { ui } from '@/theme'

const STAGES = [
  { phase: DIRECT_UPLOAD_PHASES.checksum, label: MEDIA_COPY.checksum, icon: Hash },
  { phase: DIRECT_UPLOAD_PHASES.upload, label: MEDIA_COPY.upload, icon: UploadCloud },
  { phase: DIRECT_UPLOAD_PHASES.finalize, label: MEDIA_COPY.finalize, icon: BadgeCheck },
]

function stageProgress(stageIndex, activeIndex, progress, complete) {
  if (complete || stageIndex < activeIndex) return 100
  if (stageIndex === activeIndex) return progress
  return 0
}

export function DirectUploadProgress({ phase, failedPhase, progress }) {
  const phaseBeforeError = phase === DIRECT_UPLOAD_PHASES.error ? failedPhase : phase
  const displayPhase = phaseBeforeError === DIRECT_UPLOAD_PHASES.preparing
    ? DIRECT_UPLOAD_PHASES.upload
    : phaseBeforeError
  const activeIndex = STAGES.findIndex((stage) => stage.phase === displayPhase)
  const complete = phase === DIRECT_UPLOAD_PHASES.complete

  return (
    <div
      className={`rounded-lg px-4 py-3 ${ui.card}`}
      role={phase === DIRECT_UPLOAD_PHASES.error ? 'alert' : 'status'}
      aria-live={phase === DIRECT_UPLOAD_PHASES.error ? 'assertive' : 'polite'}
      aria-atomic="true"
    >
      <div className="flex items-center justify-between gap-3">
        <p className={`font-display text-[11px] font-medium tracking-[0.18em] uppercase ${ui.eyebrow}`}>
          Tiến trình truyền
        </p>
        <p className={`text-[12px] font-medium ${phase === DIRECT_UPLOAD_PHASES.error ? ui.dangerText : ui.body}`}>
          {DIRECT_UPLOAD_PHASE_LABELS[phase]}
        </p>
      </div>
      <ol className="mt-3 grid grid-cols-3 gap-2">
        {STAGES.map((stage, index) => {
          const value = stageProgress(index, activeIndex, progress, complete)
          const highlighted = complete || index <= activeIndex
          return (
            <li key={stage.phase} className="min-w-0">
              <div className={`flex items-center gap-1.5 text-[12px] font-medium ${highlighted ? ui.accentText : ui.caption}`}>
                <Icon icon={stage.icon} />
                <span className="truncate">{stage.label}</span>
              </div>
              <div className={`mt-2 h-1 overflow-hidden rounded-md ${ui.progressTrack}`}>
                <div
                  className={`h-full ${phase === DIRECT_UPLOAD_PHASES.error && index === activeIndex ? ui.progressFailed : ui.progressFill}`}
                  style={{ width: `${value}%` }}
                  role="progressbar"
                  aria-label={stage.label}
                  aria-valuemin="0"
                  aria-valuemax="100"
                  aria-valuenow={value}
                  aria-valuetext={
                    phase === DIRECT_UPLOAD_PHASES.error && index === activeIndex
                      ? `${stage.label}: có lỗi`
                      : `${stage.label}: ${value}%`
                  }
                  aria-invalid={
                    phase === DIRECT_UPLOAD_PHASES.error && index === activeIndex
                      ? 'true'
                      : undefined
                  }
                />
              </div>
            </li>
          )
        })}
      </ol>
    </div>
  )
}
