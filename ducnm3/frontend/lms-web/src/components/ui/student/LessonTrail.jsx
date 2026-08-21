import { Check, CircleDotDashed, Bookmark } from 'lucide-react'
import { studentUi } from '@/theme/student'

const stateDetails = {
  completed: { label: 'Đã hoàn thành', icon: Check, className: studentUi.complete },
  current: { label: 'Đang học', icon: CircleDotDashed, className: studentUi.current },
  next: { label: 'Bài tiếp theo', icon: Bookmark, className: studentUi.next },
}

export function LessonTrail({ items = [] }) {
  if (items.length === 0) return null

  return (
    <ol aria-label="Lộ trình bài học" className="grid gap-3">
      {items.map((item, index) => {
        const state = stateDetails[item.state]
        const StateIcon = state.icon
        return (
          <li className={`student-reveal ${studentUi.trailItem}`} key={item.title} style={{ '--student-reveal-delay': `${index * 40}ms` }}>
            <span aria-hidden="true" className={`grid size-9 shrink-0 place-items-center rounded-xl ${state.className}`}><StateIcon size={18} strokeWidth={2} /></span>
            <div className="min-w-0">
              <p className={studentUi.trailTitle}>{item.title}</p>
              <p className={studentUi.caption}>{state.label}</p>
            </div>
          </li>
        )
      })}
    </ol>
  )
}
