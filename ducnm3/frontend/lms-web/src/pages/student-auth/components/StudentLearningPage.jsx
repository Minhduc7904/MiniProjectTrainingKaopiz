import { ArrowLeft, CheckCircle2, ChevronLeft, ChevronRight, Circle, FileText } from 'lucide-react'
import { useEffect, useState } from 'react'
import { Link, useNavigate, useParams } from 'react-router-dom'
import { useDispatch, useSelector } from 'react-redux'
import { getMediaTypeIcon } from '@/components/media/mediaPresentation'
import { StudentCourseThumbnail } from '@/components/ui/student'
import { APP_ROUTES } from '@/constants/appRoutes'
import { completeStudentLesson, fetchStudentEnrollmentDetail, fetchStudentLessonDetail } from '@/features/studentLearning/studentLearningSlice'
import { studentUi } from '@/theme/student'
import { StudentLessonMediaPreview } from './StudentLessonMediaPreview'

function completed(lesson) {
  return lesson?.progressPercent >= 100 && Boolean(lesson.completedAtUtc)
}

export function StudentLearningPage() {
  const { courseId, lessonId } = useParams()
  const navigate = useNavigate()
  const dispatch = useDispatch()
  const courseRequest = useSelector((state) => state.studentLearning.detail)
  const lessonRequest = useSelector((state) => state.studentLearning.lessonDetail)
  const completion = useSelector((state) => state.studentLearning.completion)
  const [activeMediaId, setActiveMediaId] = useState('content')
  const course = courseRequest.data
  const lessons = course?.lessons ?? []
  const lessonIndex = lessons.findIndex((item) => item.id === lessonId)
  const currentLesson = lessons[lessonIndex]
  const attachments = lessonRequest.data?.attachments ?? []
  const activeMedia = attachments.find((item) => item.usageId === activeMediaId) ?? null
  const completedLessons = lessons.filter(completed).length
  const progressPercent = lessons.length ? Math.round((completedLessons * 10000) / lessons.length) / 100 : 0

  useEffect(() => { if (courseId) void dispatch(fetchStudentEnrollmentDetail(courseId)) }, [courseId, dispatch])
  useEffect(() => { if (courseId && lessonId) { setActiveMediaId('content'); void dispatch(fetchStudentLessonDetail({ courseId, lessonId })) } }, [courseId, dispatch, lessonId])

  const learningPath = (nextLessonId) => APP_ROUTES.studentCourseLearn.replace(':courseId', courseId).replace(':lessonId', nextLessonId)
  const goToLesson = (nextLessonId) => navigate(learningPath(nextLessonId))
  const goPrevious = () => { if (lessonIndex > 0) goToLesson(lessons[lessonIndex - 1].id) }
  const goNext = () => { if (lessonIndex >= 0 && lessonIndex < lessons.length - 1) goToLesson(lessons[lessonIndex + 1].id) }

  return <main className={studentUi.learningPage}>
    <header className={studentUi.learningHeader}>
      <Link to={APP_ROUTES.studentCourseDetail.replace(':courseId', courseId)} className={`inline-flex min-h-11 shrink-0 cursor-pointer items-center gap-2 rounded-2xl border border-student-line px-3 text-sm font-bold text-student-primary-strong outline-none hover:bg-student-surface-muted focus-visible:ring-4 focus-visible:ring-student-primary/15`}><ArrowLeft size={18} /><span className="hidden sm:inline">Quay lại khóa học</span></Link>
      <div className="min-w-0 flex-1"><p className="truncate font-student-display text-sm font-bold text-student-ink sm:text-base">{course?.name ?? 'Đang tải khóa học...'}</p></div>
      <div className="hidden min-w-44 items-center gap-3 lg:flex"><span className={studentUi.caption}>Tiến độ</span><span className={`flex-1 ${studentUi.learningProgressTrack}`}><span className={studentUi.learningProgressFill} style={{ transform: `scaleX(${progressPercent / 100})` }} /></span><span className={`font-student-display text-sm font-bold text-student-primary-strong tabular-nums`}>{progressPercent}%</span></div>
      <span className={`shrink-0 ${studentUi.caption}`}>Bài {lessonIndex >= 0 ? lessonIndex + 1 : 0}/{lessons.length}</span>
      <button type="button" aria-label="Bài trước" disabled={lessonIndex <= 0} onClick={goPrevious} className={`grid size-10 cursor-pointer place-items-center rounded-xl text-student-primary-strong outline-none hover:bg-student-surface-muted focus-visible:ring-4 focus-visible:ring-student-primary/15 disabled:cursor-not-allowed disabled:opacity-40`}><ChevronLeft size={21} /></button>
      <button type="button" aria-label="Bài tiếp theo" disabled={lessonIndex < 0 || lessonIndex >= lessons.length - 1} onClick={goNext} className={`grid size-10 cursor-pointer place-items-center rounded-xl text-student-primary-strong outline-none hover:bg-student-surface-muted focus-visible:ring-4 focus-visible:ring-student-primary/15 disabled:cursor-not-allowed disabled:opacity-40`}><ChevronRight size={21} /></button>
    </header>

    <div className="grid h-[calc(100svh-4rem)] min-h-0 grid-cols-[12rem_minmax(0,1fr)_5rem] sm:grid-cols-[15rem_minmax(0,1fr)_6rem] lg:grid-cols-[20rem_minmax(0,1fr)_7rem]">
      <aside className={`${studentUi.learningSidebar} flex flex-col`} aria-label="Khu vực bài học">
        <div className="shrink-0 border-b border-student-line p-4"><p className={studentUi.eyebrow}>Nội dung khóa học</p><p className={`mt-1 ${studentUi.caption}`}>{lessons.length} bài học</p></div>
        <nav className={studentUi.learningSidebarList} aria-label="Danh sách bài học">{courseRequest.loading ? <p className={`p-4 ${studentUi.caption}`}>Đang tải bài học...</p> : lessons.map((item, index) => <button key={item.id} type="button" onClick={() => goToLesson(item.id)} className={`${studentUi.learningLesson} ${item.id === lessonId ? studentUi.learningLessonActive : ''}`}><span aria-hidden="true" className={`grid size-6 shrink-0 place-items-center rounded-full ${completed(item) ? studentUi.complete : item.id === lessonId ? studentUi.current : studentUi.next}`}>{completed(item) ? <CheckCircle2 size={15} /> : item.id === lessonId ? <Circle size={14} /> : <span className="text-[10px]">{index + 1}</span>}</span><span className="line-clamp-2 min-w-0 flex-1">{item.title}</span></button>)}</nav>
        <div className="shrink-0 border-t border-student-line p-4"><p className={`font-student-display text-sm font-bold text-student-ink`}>Tiến độ khóa học</p><p className={`mt-2 ${studentUi.caption}`}>{completedLessons}/{lessons.length} bài đã hoàn thành</p><span className={`mt-3 block ${studentUi.learningProgressTrack}`}><span className={studentUi.learningProgressFill} style={{ transform: `scaleX(${progressPercent / 100})` }} /></span></div>
      </aside>

      <section className={studentUi.learningContent} aria-label="Nội dung bài học">
        <div className="mx-auto min-h-full w-full max-w-5xl p-5 sm:p-8 lg:p-12">
          {lessonRequest.loading ? <p className={studentUi.body}>Đang tải nội dung bài học...</p> : null}
          {lessonRequest.error ? <p className={studentUi.error}>{lessonRequest.error.message}</p> : null}
          {!lessonRequest.loading && !lessonRequest.error && activeMediaId === 'content' && lessonRequest.data ? <article><p className={studentUi.eyebrow}>Bài {lessonRequest.data.displayOrder}</p><h1 className={`mt-2 ${studentUi.title}`}>{lessonRequest.data.title}</h1><div className={`mt-8 ${studentUi.learningMarkdown}`} dangerouslySetInnerHTML={{ __html: lessonRequest.data.contentHtml || '<p>Bài học chưa có nội dung.</p>' }} />{currentLesson ? <div className="mt-10 border-t border-student-line pt-6"><button type="button" disabled={completion.loading || completed(currentLesson)} onClick={() => void dispatch(completeStudentLesson({ courseId, lessonId }))} className={`student-press inline-flex min-h-11 cursor-pointer items-center gap-2 rounded-2xl px-4 font-bold outline-none focus-visible:ring-4 focus-visible:ring-student-primary/20 disabled:cursor-not-allowed disabled:opacity-55 ${studentUi.buttonPrimary}`}><CheckCircle2 size={18} />{completed(currentLesson) ? 'Đã hoàn thành' : completion.loading ? 'Đang lưu...' : 'Hoàn thành bài học'}</button>{completion.error ? <p className={`mt-3 ${studentUi.error}`}>{completion.error.message}</p> : null}</div> : null}</article> : null}
          {!lessonRequest.loading && !lessonRequest.error && activeMedia ? <StudentLessonMediaPreview media={activeMedia} /> : null}
        </div>
      </section>

      <aside className={studentUi.learningRail} aria-label="Media bài học">
        <button type="button" aria-label="Nội dung" aria-pressed={activeMediaId === 'content'} onClick={() => setActiveMediaId('content')} className={`${studentUi.learningRailButton} ${activeMediaId === 'content' ? studentUi.learningRailButtonActive : ''}`}><FileText size={22} /><span>Nội dung</span></button>
        {attachments.map((media) => { const MediaIcon = getMediaTypeIcon(media.mediaType); const selected = activeMediaId === media.usageId; return <button key={media.usageId} type="button" aria-label={`Xem ${media.originalFileName}`} aria-pressed={selected} onClick={() => setActiveMediaId(media.usageId)} className={`${studentUi.learningRailButton} mt-2 ${selected ? studentUi.learningRailButtonActive : ''}`}>{media.thumbnailUrl ? <span className="h-12 w-full overflow-hidden rounded-xl"><StudentCourseThumbnail className="rounded-xl" alt="" contentUrl={media.thumbnailUrl} /></span> : <span className="grid h-12 w-full place-items-center rounded-xl bg-student-surface-muted"><MediaIcon size={22} /></span>}<span className="line-clamp-2">{media.originalFileName || media.mediaType}</span></button> })}
      </aside>
    </div>
  </main>
}
