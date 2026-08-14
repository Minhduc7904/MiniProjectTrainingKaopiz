import { Icon } from '@/components/ui/Icon'
import { ui } from '@/theme'

export function Tabs({ tabs, value, onChange }) {
  return (
    <div role="tablist" className={ui.tabList}>
      {tabs.map((tab) => {
        const active = tab.id === value

        return (
          <button
            key={tab.id}
            type="button"
            role="tab"
            aria-selected={active}
            onClick={() => onChange(tab.id)}
            className={[
              'inline-flex cursor-pointer items-center gap-1.5 px-3 py-2.5 text-[13px] font-medium',
              active ? ui.tabActive : ui.tabIdle,
            ].join(' ')}
          >
            {tab.icon ? <Icon icon={tab.icon} /> : null}
            {tab.label}
          </button>
        )
      })}
    </div>
  )
}
