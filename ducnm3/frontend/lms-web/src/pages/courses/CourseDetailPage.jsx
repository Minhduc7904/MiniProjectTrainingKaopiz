import { useEffect, useRef, useState } from 'react'
import { Link, useParams } from 'react-router-dom'
import { ArrowLeft, ImagePlus, Plus, Trash2 } from 'lucide-react'
import { Button } from '@/components/ui/Button'
import { Icon } from '@/components/ui/Icon'
import { EmptyState } from '@/components/ui/EmptyState'
import { LoadingState } from '@/components/ui/LoadingState'
import { Pagination } from '@/components/ui/Pagination'
import { PageHeader } from '@/components/ui/PageHeader'
import { Dropdown } from '@/components/ui/Dropdown'
import { InputPanel } from '@/components/layout/InputPanel'
import { OutputPanel } from '@/components/layout/OutputPanel'
import { Workbench } from '@/components/layout/Workbench'
import { MediaImagePreview } from '@/pages/media/components/MediaImagePreview'
import { MediaLibraryModal } from '@/components/media/MediaLibraryModal'
import { createMediaUsageRequest, createMediaUsagesBatchRequest, removeMediaUsageRequest, reorderMediaUsagesRequest } from '@/api/mediaApi'
import { createCourseLessonRequest } from '@/api/coursesApi'
import { RightPanel } from '@/components/layout/RightPanel'
import { MarkdownEditor } from '@/components/markdown/MarkdownEditor'
import { useDispatch, useSelector } from 'react-redux'
import { fetchCourseDetails, fetchCoursesList } from '@/features/courses/coursesSlice'
import { APP_ROUTES } from '@/constants/appRoutes'
import { COURSE_STATUS, COURSE_STATUS_LABELS } from '@/constants/courseStatus'
import { COURSE_SORT_BY } from '@/constants/inputs/getCourses'
import { SORT_DIRECTIONS } from '@/constants/queryParams'
import { GET_COURSES_ACTIVITY } from '@/constants/activities/getCourses'
import { ui } from '@/theme'

export function CourseDetailPage() {
  const { courseId } = useParams()
  const dispatch = useDispatch()
  const detail = useSelector((state) => state.courses.detail)
  const [mediaMode, setMediaMode] = useState(null)
  const [savingMedia, setSavingMedia] = useState(false)
  const [mediaError, setMediaError] = useState(null)
  const [selectedGalleryIndex, setSelectedGalleryIndex] = useState(0)
  const [draggedGalleryIndex, setDraggedGalleryIndex] = useState(null)
  const [lessonPanelOpen, setLessonPanelOpen] = useState(false)
  const [lessonSaving, setLessonSaving] = useState(false)
  const [lessonError, setLessonError] = useState(null)
  const [lessonForm, setLessonForm] = useState({ title: '', contentMarkdown: '' })
  useEffect(() => { dispatch(fetchCourseDetails(courseId)) }, [dispatch, courseId])
  useEffect(() => { setSelectedGalleryIndex(0) }, [courseId, detail.data?.gallery?.length])

  if (detail.loading) return <main className={`min-h-svh p-8 ${ui.page}`}><LoadingState label="Đang tải khóa học..." /></main>
  if (detail.error) return <main className={`min-h-svh p-8 ${ui.page}`}><EmptyState title="Không tải được khóa học" description={detail.error.message} /></main>
  if (!detail.data) return null
  const course = detail.data
  const gallery = course.gallery ?? []
  const selectedGallery = gallery[selectedGalleryIndex] ?? gallery[0] ?? null
  const openMedia = (mode) => {
    setMediaError(null)
    setMediaMode(mode)
  }
  const saveMedia = async (selected) => {
    setSavingMedia(true)
    setMediaError(null)
    try {
      if (mediaMode === 'thumbnail') {
        const thumbnailMediaId = selected[0]?.thumbnailMediaId
        if (!thumbnailMediaId) throw new Error('Thumbnail WebP chưa READY. Hãy chọn ảnh đã xử lý xong.')
        await createMediaUsageRequest({
          mediaId: thumbnailMediaId,
          ownerService: 'COURSE',
          ownerType: 'COURSE_THUMBNAIL',
          ownerId: courseId,
          usageType: 'THUMBNAIL',
          displayOrder: 0,
        })
      } else {
        await createMediaUsagesBatchRequest(selected.map((media, index) => ({
          mediaId: media.id,
          ownerService: 'COURSE',
          ownerType: 'COURSE_GALLERY',
          ownerId: courseId,
          usageType: 'ATTACHMENT',
          displayOrder: index,
        })))
      }
      setMediaMode(null)
      dispatch(fetchCourseDetails(courseId))
    } catch (error) {
      setMediaError(error?.message ?? 'Không thể cập nhật media khóa học.')
    } finally {
      setSavingMedia(false)
    }
  }
  const removeGalleryMedia = async (usageId) => {
    setSavingMedia(true)
    setMediaError(null)
    try {
      await removeMediaUsageRequest(usageId)
      dispatch(fetchCourseDetails(courseId))
    } catch (error) {
      setMediaError(error?.message ?? 'Không thể gỡ media khỏi gallery.')
    } finally {
      setSavingMedia(false)
    }
  }
  const removeCourseThumbnail = async () => {
    if (!course.thumbnail?.usageId) return
    setSavingMedia(true)
    setMediaError(null)
    try {
      await removeMediaUsageRequest(course.thumbnail.usageId)
      dispatch(fetchCourseDetails(courseId))
    } catch (error) {
      setMediaError(error?.message ?? 'Không thể gỡ thumbnail khóa học.')
    } finally {
      setSavingMedia(false)
    }
  }
  const moveGalleryMedia = async (index, direction) => {
    const nextIndex = index + direction
    if (nextIndex < 0 || nextIndex >= course.gallery.length) return
    const usageIds = course.gallery.map((media) => media.usageId)
    ;[usageIds[index], usageIds[nextIndex]] = [usageIds[nextIndex], usageIds[index]]
    setSavingMedia(true)
    setMediaError(null)
    try {
      await reorderMediaUsagesRequest({
        ownerService: 'COURSE',
        ownerType: 'COURSE_GALLERY',
        ownerId: courseId,
        usageIds,
      })
      dispatch(fetchCourseDetails(courseId))
    } catch (error) {
      setMediaError(error?.message ?? 'Không thể đổi thứ tự gallery.')
    } finally {
      setSavingMedia(false)
    }
  }
  const dropGalleryMedia = (targetIndex) => {
    if (draggedGalleryIndex === null || draggedGalleryIndex === targetIndex) return
    void moveGalleryMedia(draggedGalleryIndex, targetIndex - draggedGalleryIndex)
    setDraggedGalleryIndex(null)
  }
  const submitLesson = async (event) => {
    event.preventDefault()
    setLessonSaving(true)
    setLessonError(null)
    try {
      await createCourseLessonRequest(courseId, lessonForm)
      setLessonForm({ title: '', contentMarkdown: '' })
      setLessonPanelOpen(false)
      dispatch(fetchCourseDetails(courseId))
    } catch (error) {
      setLessonError(error?.message ?? 'Không thể thêm lesson.')
    } finally {
      setLessonSaving(false)
    }
  }
  return (
    <main className={`h-screen min-h-screen overflow-y-auto px-5 py-6 md:px-8 md:py-8 ${ui.page}`}>
            <div className="mx-auto max-w-6xl">
              <Link to={APP_ROUTES.courses} className={`inline-flex items-center gap-2 text-[13px] ${ui.body}`}><Icon icon={ArrowLeft} />Danh sách khóa học</Link>
              <section className="mt-6 grid items-start gap-7 lg:grid-cols-[minmax(220px,300px)_minmax(0,1fr)]">
                <div className={`group relative overflow-hidden rounded-lg ${ui.card}`}>
                  <div className="aspect-[4/3] max-h-72 bg-surface-muted">
                    {course.thumbnail?.contentUrl ? <MediaImagePreview contentUrl={course.thumbnail.contentUrl} alt={course.name} /> : <button type="button" className={`flex h-full w-full flex-col items-center justify-center gap-2 ${ui.body}`} onClick={() => openMedia('thumbnail')}><Icon icon={ImagePlus} size={28} /><span className="text-[12px]">Thêm thumbnail</span></button>}
                  </div>
                  {course.thumbnail ? <button type="button" aria-label="Gỡ thumbnail" disabled={savingMedia} onClick={() => void removeCourseThumbnail()} className="absolute right-2 top-2 rounded-full bg-black/65 p-2 text-white"><Icon icon={Trash2} size={15} /></button> : null}
                  {course.thumbnail ? <button type="button" aria-label="Gỡ thumbnail khi hover" disabled={savingMedia} onClick={() => void removeCourseThumbnail()} className="absolute left-1/2 top-1/2 -translate-x-1/2 -translate-y-1/2 rounded-full bg-black/70 p-3 text-white opacity-0 transition group-hover:opacity-100"><Icon icon={Trash2} size={21} /></button> : null}
                </div>
                <div className="min-w-0">
                  <p className={`text-[11px] font-medium tracking-[0.2em] uppercase ${ui.eyebrow}`}>Course detail</p>
                  <h1 className={`mt-2 max-w-3xl font-display text-[30px] font-semibold leading-tight ${ui.title}`}>{course.name}</h1>
                  <p className={`mt-3 text-[14px] ${ui.body}`}>{course.status} · Tạo ngày {new Date(course.createdAtUtc).toLocaleDateString('vi-VN')}</p>
                  <p className={`mt-5 max-w-2xl text-[14px] leading-6 ${ui.body}`}>Khóa học gồm {course.lessons.length} bài học. Khám phá nội dung và thư viện media được sắp xếp theo trình tự học tập.</p>
                </div>
              </section>

              <section className="mt-10">
                <div className="flex items-end justify-between gap-4"><div><p className={`text-[11px] font-medium tracking-[0.2em] uppercase ${ui.eyebrow}`}>Media library</p><h2 className={`mt-1 font-display text-[22px] font-semibold ${ui.title}`}>Gallery khóa học</h2><p className={`mt-1 text-[12px] ${ui.caption}`}>Kéo thumbnail và thả vào ô sáng để đổi vị trí.</p></div><Button type="button" disabled={savingMedia} onClick={() => openMedia('gallery')}><Icon icon={ImagePlus} />Thêm ảnh</Button></div>
                <div className="mt-4 grid gap-4 lg:grid-cols-[190px_minmax(0,1fr)]">
                  <div className="flex gap-2 overflow-x-auto pb-2 lg:max-h-[390px] lg:flex-col lg:overflow-y-auto lg:pr-2">
                    {gallery.length ? gallery.map((media, index) => <div key={media.usageId} draggable={!savingMedia} onDragStart={() => setDraggedGalleryIndex(index)} onDragEnd={() => setDraggedGalleryIndex(null)} onDragOver={(event) => event.preventDefault()} onDrop={() => dropGalleryMedia(index)} onClick={() => setSelectedGalleryIndex(index)} className={`group relative shrink-0 cursor-grab overflow-hidden rounded-md border transition ${draggedGalleryIndex === index ? 'scale-95 border-accent opacity-50 ring-2 ring-accent/40' : selectedGallery?.usageId === media.usageId ? 'border-accent ring-2 ring-accent/30' : 'border-line'} bg-surface-muted active:cursor-grabbing`}><div className="aspect-[4/3] lg:aspect-video"><MediaImagePreview contentUrl={media.thumbnailUrl || media.contentUrl} alt="" /></div><span className="absolute left-2 top-2 rounded bg-black/55 px-1.5 py-0.5 text-[10px] text-white">#{index + 1}</span><button type="button" aria-label="Gỡ ảnh khỏi gallery" disabled={savingMedia} onClick={(event) => { event.stopPropagation(); void removeGalleryMedia(media.usageId) }} className="absolute right-1.5 top-1.5 rounded-full bg-black/60 p-1 text-white opacity-100 transition group-hover:scale-105"><Icon icon={Trash2} size={13} /></button><button type="button" aria-label="Xóa ảnh đang hover" disabled={savingMedia} onClick={(event) => { event.stopPropagation(); void removeGalleryMedia(media.usageId) }} className="absolute left-1/2 top-1/2 -translate-x-1/2 -translate-y-1/2 rounded-full bg-black/65 p-2 text-white opacity-0 transition group-hover:opacity-100"><Icon icon={Trash2} size={17} /></button>{draggedGalleryIndex !== null && draggedGalleryIndex !== index ? <span className="pointer-events-none absolute inset-x-2 bottom-2 rounded bg-accent px-2 py-1 text-center text-[10px] font-medium text-white">Thả để đặt tại đây</span> : null}</div>) : <button type="button" onClick={() => openMedia('gallery')} className={`flex aspect-[4/3] w-32 shrink-0 flex-col items-center justify-center gap-2 rounded-md border border-dashed ${ui.body} lg:w-full`}><Icon icon={ImagePlus} size={25} /><span className="text-[11px]">Tải ảnh lên</span></button>}
                  </div>
                  <div className={`min-h-[280px] overflow-hidden rounded-lg ${ui.card}`}>
                    {selectedGallery ? <a href={selectedGallery.contentUrl} target="_blank" rel="noreferrer" className="block h-full min-h-[280px] bg-surface-muted"><MediaImagePreview contentUrl={selectedGallery.contentUrl} alt={course.name} /></a> : <div className={`flex min-h-[280px] items-center justify-center text-[13px] ${ui.body}`}>Chọn ảnh gallery để xem media chính</div>}
                  </div>
                </div>
                {mediaError ? <p className={`mt-3 rounded-md px-3 py-2 text-[12px] ${ui.badgeDanger}`}>{mediaError}</p> : null}
              </section>

              <section className="mt-10 pb-8"><div className="flex items-end justify-between gap-4"><div><p className={`text-[11px] font-medium tracking-[0.2em] uppercase ${ui.eyebrow}`}>Course content</p><h2 className={`mt-1 font-display text-[22px] font-semibold ${ui.title}`}>Lessons</h2></div><Button type="button" onClick={() => { setLessonError(null); setLessonPanelOpen(true) }}><Icon icon={Plus} />Thêm lesson</Button></div><div className="mt-4 space-y-3">{course.lessons.map((lesson, index) => <article key={lesson.id} className={`flex items-start gap-4 rounded-lg p-4 ${ui.card}`}><span className={`font-mono text-[12px] ${ui.caption}`}>{String(index + 1).padStart(2, '0')}</span><div><h3 className={`text-[15px] font-medium ${ui.title}`}>{lesson.title}</h3><p className={`mt-1 text-[12px] ${ui.body}`}>{lesson.progresses?.length ?? 0} lượt tiến độ</p></div></article>)}</div></section>
            </div>
      <RightPanel open={lessonPanelOpen} title="Thêm lesson" description={`Lesson mới sẽ được thêm vào cuối khóa học.`} onClose={() => { if (!lessonSaving) setLessonPanelOpen(false) }} footer={<div className="flex justify-end gap-2"><Button type="button" variant="ghost" disabled={lessonSaving} onClick={() => setLessonPanelOpen(false)}>Hủy</Button><Button type="submit" form="create-lesson-form" disabled={lessonSaving || !lessonForm.title.trim()}>Lưu lesson</Button></div>}>
        <form id="create-lesson-form" className="space-y-4" onSubmit={submitLesson}>
          <label className={`block text-[13px] ${ui.body}`}>Tiêu đề lesson<input className="mt-1 w-full rounded-md border border-line bg-transparent px-3 py-2 text-[14px]" maxLength={200} value={lessonForm.title} onChange={(event) => setLessonForm((current) => ({ ...current, title: event.target.value }))} placeholder="Ví dụ: Tổng quan về hệ thống" /></label>
          <MarkdownEditor value={lessonForm.contentMarkdown} onChange={(contentMarkdown) => setLessonForm((current) => ({ ...current, contentMarkdown }))} placeholder="Nội dung lesson (không bắt buộc)" />
          {lessonError ? <p className={`rounded-md px-3 py-2 text-[12px] ${ui.badgeDanger}`}>{lessonError}</p> : null}
        </form>
      </RightPanel>
      <MediaLibraryModal
        open={mediaMode !== null}
        selectionMode={mediaMode === 'gallery' ? 'multiple' : 'single'}
        initialSelection={mediaMode === 'gallery' ? course.gallery?.map((media) => ({ id: media.mediaId })) : []}
        onClose={() => { if (!savingMedia) setMediaMode(null) }}
        onConfirm={saveMedia}
      />
    </main>
  )
}

export function CoursesPage() {
  const dispatch = useDispatch()
  const list = useSelector((state) => state.courses.list)
  const loaded = useRef(false)

  useEffect(() => {
    if (!loaded.current) {
      loaded.current = true
      dispatch(fetchCoursesList(list.query))
    }
  }, [dispatch, list.query])

  const load = (patch) => dispatch(fetchCoursesList({ ...list.query, ...patch }))

  return (
    <Workbench
      input={
        <InputPanel
          guided={
            <div className="flex h-full min-h-0 flex-col">
              <div className="min-h-0 flex-1 overflow-y-auto px-5 py-4">
                <PageHeader eyebrow="Course" title="Danh sách khóa học" description="Lọc và phân trang dữ liệu Course." />
                <div className="mt-5 flex flex-col gap-3">
                  <Dropdown label="Trạng thái" value={list.query.status ?? ''} disabled={list.loading} options={[{ value: '', label: 'Tất cả' }, ...Object.values(COURSE_STATUS).map((value) => ({ value, label: COURSE_STATUS_LABELS[value] }))]} onChange={(status) => load({ status: status || undefined, page: 1 })} />
                  <Dropdown label="Sắp xếp" value={list.query.sortBy} disabled={list.loading} options={[{ value: COURSE_SORT_BY.createdAt, label: 'Ngày tạo' }, { value: COURSE_SORT_BY.name, label: 'Tên khóa học' }]} onChange={(sortBy) => load({ sortBy, page: 1 })} />
                  <Dropdown label="Thứ tự" value={list.query.sortDirection} disabled={list.loading} options={[{ value: SORT_DIRECTIONS.desc, label: 'Giảm dần' }, { value: SORT_DIRECTIONS.asc, label: 'Tăng dần' }]} onChange={(sortDirection) => load({ sortDirection, page: 1 })} />
                </div>
              </div>
              <div className={`shrink-0 px-5 py-3 ${ui.hairlineT}`}><Pagination page={list.query.page} totalPages={list.pagination.totalPages} totalItems={list.pagination.totalItems} pageSize={list.query.pageSize} loading={list.loading} itemLabel="khóa học" onPageChange={(page) => load({ page })} onPageSizeChange={(pageSize) => load({ page: 1, pageSize })} /></div>
            </div>
          }
          manual={<pre className={`m-5 overflow-auto rounded-md p-3 ${ui.code}`}>{JSON.stringify(list.query, null, 2)}</pre>}
        />
      }
      output={
        <OutputPanel json={{ data: list.data, error: list.error, meta: { pagination: list.pagination, traceId: list.traceId, query: list.query } }} activity={GET_COURSES_ACTIVITY} run={list}>
          {list.error ? <EmptyState title="Không tải được khóa học" description={list.error.message} /> : null}
          {!list.error && list.loading && !list.data.length ? <LoadingState label="Đang tải khóa học..." /> : null}
          {!list.error && !list.loading && !list.data.length ? <EmptyState title="Chưa có khóa học" description="Không có dữ liệu phù hợp." /> : null}
          {list.data.length ? <div className="grid gap-4 sm:grid-cols-2 xl:grid-cols-3">{list.data.map((course) => <Link key={course.id} to={APP_ROUTES.courseDetails.replace(':courseId', course.id)} className={`overflow-hidden rounded-lg ${ui.card}`}><div className="aspect-video bg-surface-muted">{course.thumbnailUrl ? <MediaImagePreview contentUrl={course.thumbnailUrl} alt={course.name} /> : <div className={`flex h-full items-center justify-center text-[12px] ${ui.caption}`}>Chưa có thumbnail</div>}</div><div className="p-4"><h2 className={`truncate text-[15px] font-medium ${ui.title}`}>{course.name}</h2><p className={`mt-2 text-[12px] ${ui.body}`}>{course.status}</p></div></Link>)}</div> : null}
        </OutputPanel>
      }
    />
  )
}
