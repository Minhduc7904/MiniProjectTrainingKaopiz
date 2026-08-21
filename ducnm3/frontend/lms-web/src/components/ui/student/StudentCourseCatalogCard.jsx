import { BookPlus, LoaderCircle } from 'lucide-react'
import { studentUi } from '@/theme/student'
import { StudentCourseThumbnail } from './StudentCourseThumbnail'

export function StudentCourseCatalogCard({ course, enrolling = false, onEnroll }) {
  return (
    <article className={studentUi.catalogCard}>
      <StudentCourseThumbnail alt={course.name} contentUrl={course.thumbnailUrl} />
      <div className="grid gap-4 p-5">
        <div className="grid gap-2"><span className={studentUi.catalogStatus}>{course.status}</span><h2 className={studentUi.courseTitle}>{course.name}</h2><p className={studentUi.courseHint}>Sẵn sàng để bắt đầu hành trình học mới.</p></div>
        <button type="button" className={studentUi.catalogEnrollButton} disabled={enrolling} aria-label={`Đăng kí ${course.name}`} onClick={() => onEnroll(course.courseId)}>{enrolling ? <LoaderCircle aria-hidden="true" className="animate-spin" size={18} /> : <BookPlus aria-hidden="true" size={18} />}{enrolling ? 'Đang đăng kí...' : 'Đăng kí'}</button>
      </div>
    </article>
  )
}
