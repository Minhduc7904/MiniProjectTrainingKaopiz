import { useEffect } from 'react'
import { useDispatch, useSelector } from 'react-redux'
import { useNavigate } from 'react-router-dom'
import { BookOpen } from 'lucide-react'
import { StudentAccountMenu, StudentCard, StudentCourseCatalogCard, StudentNavigation, StudentPagination, StudentShell } from '@/components/ui/student'
import { APP_ROUTES } from '@/constants/appRoutes'
import { enrollStudentCourse, fetchStudentCourseCatalog, STUDENT_COURSE_CATALOG_DEFAULT_QUERY } from '@/features/studentLearning/studentLearningSlice'
import { studentUi } from '@/theme/student'

export function StudentCourseCatalog({ student }) {
  const dispatch = useDispatch()
  const navigate = useNavigate()
  const catalog = useSelector((state) => state.studentLearning.catalog)
  const enrollmentByCourseId = useSelector((state) => state.studentLearning.enrollmentByCourseId)

  useEffect(() => { void dispatch(fetchStudentCourseCatalog(STUDENT_COURSE_CATALOG_DEFAULT_QUERY)) }, [dispatch])

  function movePage(page) {
    void dispatch(fetchStudentCourseCatalog({ ...catalog.query, page }))
  }

  async function enroll(courseId) {
    try {
      await dispatch(enrollStudentCourse(courseId)).unwrap()
      navigate(APP_ROUTES.studentCourseDetail.replace(':courseId', courseId), { replace: true })
    } catch {
      // Redux keeps the safe API error beside the Course card.
    }
  }

  return (
    <StudentShell navigation={<StudentNavigation />} action={<StudentAccountMenu displayName={student.displayName} email={student.email} logoutTo={APP_ROUTES.studentLogout} profileTo={APP_ROUTES.studentProfile} />}>
      <section className="grid gap-7 py-4 sm:gap-9 sm:py-10">
        <div className="student-enter grid max-w-2xl gap-3"><p className={studentUi.eyebrow}>Khám phá</p><h1 className={studentUi.title}>Khóa học</h1><p className={studentUi.body}>Chọn một khóa học mới để bắt đầu. Những khóa học bạn đã ghi danh sẽ không xuất hiện ở đây.</p></div>
        {catalog.loading ? <p className={studentUi.body}>Đang tải danh mục khóa học...</p> : null}
        {catalog.error ? <p className={studentUi.error}>{catalog.error.message}</p> : null}
        {!catalog.loading && !catalog.error && catalog.data.length === 0 ? <StudentCard className="student-enter grid max-w-2xl gap-4 p-6 sm:p-8"><span aria-hidden="true" className={studentUi.emptyIcon}><BookOpen size={24} /></span><div className="grid gap-1"><h2 className={studentUi.emptyTitle}>Bạn đã khám phá hết rồi</h2><p className={studentUi.body}>Hiện không còn khóa học mới để ghi danh.</p></div></StudentCard> : null}
        {catalog.data.length > 0 ? <><div className="grid gap-5 sm:grid-cols-2 lg:grid-cols-3">{catalog.data.map((course, index) => { const enrollment = enrollmentByCourseId[course.courseId]; return <div className="student-reveal grid gap-2" key={course.courseId} style={{ '--student-reveal-delay': `${Math.min(index, 5) * 45}ms` }}><StudentCourseCatalogCard course={course} enrolling={enrollment?.loading} onEnroll={enroll} />{enrollment?.error ? <p className={studentUi.error}>{enrollment.error.message}</p> : null}</div> })}</div><StudentPagination page={catalog.pagination.page} totalPages={catalog.pagination.totalPages} onPageChange={movePage} /></> : null}
      </section>
    </StudentShell>
  )
}
