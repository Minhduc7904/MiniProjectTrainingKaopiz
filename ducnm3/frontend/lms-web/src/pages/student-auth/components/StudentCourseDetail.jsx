import { useEffect, useState } from 'react'
import { ArrowLeft, BookOpenCheck, ChevronLeft, ChevronRight, ListChecks } from 'lucide-react'
import { Link, useParams } from 'react-router-dom'
import { useDispatch, useSelector } from 'react-redux'
import { StudentAccountMenu, StudentCourseThumbnail, StudentNavigation, StudentShell } from '@/components/ui/student'
import { APP_ROUTES } from '@/constants/appRoutes'
import { fetchStudentEnrollmentDetail } from '@/features/studentLearning/studentLearningSlice'
import { studentUi } from '@/theme/student'

export function StudentCourseDetail({ student }) {
  const { courseId } = useParams()
  const dispatch = useDispatch()
  const detail = useSelector((state) => state.studentLearning.detail)
  const [galleryIndex, setGalleryIndex] = useState(0)

  useEffect(() => { if (courseId) void dispatch(fetchStudentEnrollmentDetail(courseId)) }, [courseId, dispatch])
  useEffect(() => { setGalleryIndex(0) }, [detail.data?.id])
  const gallery = detail.data?.gallery ?? []
  const selectedGallery = gallery[galleryIndex]
  const selectPrevious = () => setGalleryIndex((current) => (current + gallery.length - 1) % gallery.length)
  const selectNext = () => setGalleryIndex((current) => (current + 1) % gallery.length)

  return (
    <StudentShell navigation={<StudentNavigation />} action={<StudentAccountMenu displayName={student.displayName} email={student.email} logoutTo={APP_ROUTES.studentLogout} profileTo={APP_ROUTES.studentProfile} />}>
      <section className="grid gap-7 py-4 sm:gap-9 sm:py-10">
        <Link className={`inline-flex min-h-11 w-fit items-center gap-2 ${studentUi.link}`} to={APP_ROUTES.studentHome}><ArrowLeft aria-hidden="true" size={18} />Về Home</Link>
        {detail.loading ? <p className={studentUi.body}>Đang tải khóa học...</p> : null}
        {detail.error ? <p className={studentUi.error}>{detail.error.message}</p> : null}
        {detail.data ? <div className="grid gap-8"><div className="grid gap-6 lg:grid-cols-[minmax(0,1.1fr)_minmax(20rem,0.9fr)] lg:items-center"><StudentCourseThumbnail className="rounded-3xl" alt={detail.data.name} contentUrl={detail.data.thumbnail?.contentUrl} /><div className="student-enter grid gap-4"><p className={studentUi.eyebrow}>Khóa học đã ghi danh</p><h1 className={studentUi.title}>{detail.data.name}</h1><p className={studentUi.body}>{detail.data.status === 'PUBLISHED' ? 'Sẵn sàng để bạn khám phá nội dung.' : 'Thông tin khóa học.'}</p><button type="button" disabled className={studentUi.courseCta}><BookOpenCheck aria-hidden="true" size={19} />Vào học · sắp ra mắt</button></div></div>
          {gallery.length > 0 ? <section className={studentUi.courseDetailSection}><div className="flex items-end justify-between gap-4"><div><p className={studentUi.eyebrow}>Không gian khóa học</p><h2 className={studentUi.sectionTitle}>Thư viện hình ảnh</h2></div><span className={studentUi.caption}>{galleryIndex + 1}/{gallery.length}</span></div><div className="relative mt-4"><StudentCourseThumbnail className="rounded-3xl" alt={`${detail.data.name} - ảnh ${galleryIndex + 1}`} contentUrl={selectedGallery?.contentUrl} />{gallery.length > 1 ? <><button className={`${studentUi.galleryButton} left-3`} type="button" aria-label="Ảnh trước" onClick={selectPrevious}><ChevronLeft size={20} /></button><button className={`${studentUi.galleryButton} right-3`} type="button" aria-label="Ảnh tiếp theo" onClick={selectNext}><ChevronRight size={20} /></button></> : null}</div></section> : null}
          <section className={studentUi.courseDetailSection}><p className={studentUi.eyebrow}>Giới thiệu</p><h2 className={studentUi.sectionTitle}>Bạn sẽ học được gì?</h2><div className={`${studentUi.description} mt-4`} dangerouslySetInnerHTML={{ __html: detail.data.descriptionHtml || '<p>Khóa học chưa có mô tả.</p>' }} /></section>
          <section className={studentUi.courseDetailSection}><div className="flex items-center gap-2"><ListChecks aria-hidden="true" className={studentUi.sectionIcon} size={20} /><div><p className={studentUi.eyebrow}>Nội dung</p><h2 className={studentUi.sectionTitle}>Các bài học</h2></div></div><ol className="mt-4 grid gap-2">{detail.data.lessons.map((lesson, index) => <li className={studentUi.lessonPreview} key={lesson.id}><span className={studentUi.lessonNumber}>{String(index + 1).padStart(2, '0')}</span><span>{lesson.title}</span></li>)}</ol>{detail.data.lessons.length === 0 ? <p className={`${studentUi.body} mt-4`}>Khóa học chưa có bài học để xem trước.</p> : null}</section>
        </div> : null}
      </section>
    </StudentShell>
  )
}
