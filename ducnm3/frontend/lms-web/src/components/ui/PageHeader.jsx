import { ui } from '@/theme'

export function PageHeader({ eyebrow, title, description, actions }) {
  return (
    <header className="mb-5">
      <div>
        {eyebrow ? (
          <p
            className={`font-display text-[11px] font-medium tracking-[0.2em] uppercase ${ui.eyebrow}`}
          >
            {eyebrow}
          </p>
        ) : null}
        <h2
          className={`mt-1 font-display text-[22px] leading-none font-semibold ${ui.title}`}
        >
          {title}
        </h2>
        {description ? (
          <p className={`mt-2 max-w-[52ch] text-[14px] ${ui.body}`}>
            {description}
          </p>
        ) : null}
      </div>
      {actions ? <div className="flex items-center gap-2">{actions}</div> : null}
    </header>
  )
}
