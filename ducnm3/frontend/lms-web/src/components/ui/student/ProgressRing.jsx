import { studentUi } from '@/theme/student'

export function ProgressRing({ completed, total }) {
  const percentage = total > 0 ? Math.round((completed / total) * 100) : 0

  return (
    <div aria-label={`${percentage}% bài học hoàn thành`} className={studentUi.progressRing} role="img">
      <div className="text-center">
        <p className={studentUi.progressValue}>{percentage}%</p>
        <p className={studentUi.caption}>tiến độ</p>
      </div>
    </div>
  )
}
