import { useEffect } from 'react'
import { useDispatch, useSelector } from 'react-redux'
import { StudentAccountMenu, StudentCourseCard, StudentEmptyState, StudentNavigation, StudentPagination, StudentShell } from '@/components/ui/student'
import { APP_ROUTES } from '@/constants/appRoutes'
import { fetchStudentEnrollments, STUDENT_ENROLLMENTS_DEFAULT_QUERY } from '@/features/studentLearning/studentLearningSlice'
import { studentUi } from '@/theme/student'

export function StudentDashboard({ student }) {
  const dispatch = useDispatch()
  const enrollments = useSelector((state) => state.studentLearning.enrollments)
  const progressByCourseId = useSelector((state) => state.studentLearning.progressByCourseId)

  useEffect(() => { void dispatch(fetchStudentEnrollments(STUDENT_ENROLLMENTS_DEFAULT_QUERY)) }, [dispatch])

  function movePage(page) {
    void dispatch(fetchStudentEnrollments({ ...enrollments.query, page }))
  }

  return (
    <StudentShell navigation={<StudentNavigation />} action={<StudentAccountMenu displayName={student.displayName} email={student.email} logoutTo={APP_ROUTES.studentLogout} profileTo={APP_ROUTES.studentProfile} />}>
      <section className="grid gap-7 py-4 sm:gap-9 sm:py-10">
        <div className="student-enter grid max-w-2xl gap-3">
          <p className={studentUi.eyebrow}>Không gian học tập của bạn</p>
          <h1 className={studentUi.title}>Tiếp tục hành trình học</h1>
          <p className={`${studentUi.body} max-w-xl`}>Mỗi bài học hoàn thành sẽ đưa bạn tiến gần hơn tới mục tiêu của mình.</p>
        </div>
        {enrollments.loading ? <p className={studentUi.body}>Đang tải các khóa học đã ghi danh...</p> : null}
        {enrollments.error ? <p className={studentUi.error}>{enrollments.error.message}</p> : null}
        {!enrollments.loading && !enrollments.error && enrollments.data.length === 0 ? <StudentEmptyState /> : null}
        {enrollments.data.length > 0 ? <><div className="grid gap-5 sm:grid-cols-2 lg:grid-cols-3">{enrollments.data.map((course, index) => <article className="student-reveal" key={course.enrollmentId} style={{ '--student-reveal-delay': `${Math.min(index, 5) * 45}ms` }}><StudentCourseCard course={course} progress={progressByCourseId[course.courseId]?.data} /></article>)}</div><StudentPagination page={enrollments.pagination.page} totalPages={enrollments.pagination.totalPages} onPageChange={movePage} /></> : null}
      </section>
    </StudentShell>
  )
}
