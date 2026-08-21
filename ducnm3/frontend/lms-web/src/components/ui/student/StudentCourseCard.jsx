import { ArrowRight, CircleCheck, Clock3 } from 'lucide-react'
import { Link } from 'react-router-dom'
import { APP_ROUTES } from '@/constants/appRoutes'
import { studentUi } from '@/theme/student'
import { StudentCourseThumbnail } from './StudentCourseThumbnail'

function progressLabel(progress) {
  if (!progress) return 'Đang chuẩn bị lộ trình'
  if (progress.progressPercent >= 100) return 'Bạn đã hoàn thành khóa học'
  if (progress.nextLesson) return `Tiếp theo: ${progress.nextLesson.title}`
  return 'Đang chuẩn bị bài học tiếp theo'
}

export function StudentCourseCard({ course, progress }) {
  const percent = progress?.progressPercent ?? 0
  return (
    <Link className={`${studentUi.courseCard} student-press`} to={APP_ROUTES.studentCourseDetail.replace(':courseId', course.courseId)} aria-label={`Mở khóa học ${course.name}`}>
      <StudentCourseThumbnail alt={course.name} contentUrl={course.thumbnailUrl} />
      <div className="grid gap-4 p-5">
        <div className="grid gap-1.5">
          <p className={studentUi.courseMeta}>Đã ghi danh</p>
          <h2 className={studentUi.courseTitle}>{course.name}</h2>
          <p className={studentUi.courseHint}>{progressLabel(progress)}</p>
        </div>
        <div className="grid gap-2">
          <div className="flex items-center justify-between gap-3"><span className={studentUi.courseProgressLabel}>Tiến độ</span><span className={studentUi.courseProgressValue}>{percent}%</span></div>
          <span aria-label={`Tiến độ ${percent}%`} className={studentUi.progressTrack}><span className={studentUi.progressFill} style={{ transform: `scaleX(${percent / 100})` }} /></span>
        </div>
        <div className={studentUi.courseFooter}>
          <span className={studentUi.courseFooterText}>{percent >= 100 ? <CircleCheck aria-hidden="true" size={16} /> : <Clock3 aria-hidden="true" size={16} />}{progress?.completedLessons ?? 0}/{progress?.totalLessons ?? 0} bài học</span>
          <span className={studentUi.courseOpen}>Xem khóa học <ArrowRight aria-hidden="true" size={16} /></span>
        </div>
      </div>
    </Link>
  )
}
