import { useEffect, useRef, useState } from 'react'
import { Link, useNavigate, useParams } from 'react-router-dom'
import { ArrowLeft, File, FileText, GripVertical, Image, ImagePlus, Music2, Pencil, Plus, Trash2, Video } from 'lucide-react'
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
import { MediaPreviewModal } from '@/components/media/MediaPreviewModal'
import { createMediaUsageRequest, createMediaUsagesBatchRequest, removeMediaUsageRequest, reorderMediaUsagesRequest } from '@/api/mediaApi'
import { createCourseLessonRequest, createCourseRequest, deleteCourseLessonRequest, deleteCourseRequest, fetchCourseLessonDetailRequest, reorderCourseLessonsRequest, updateCourseLessonRequest, updateCourseRequest } from '@/api/coursesApi'
import { RightPanel } from '@/components/layout/RightPanel'
import { MarkdownEditor } from '@/components/markdown/MarkdownEditor'
import { RenderedMarkdown } from '@/components/markdown/RenderedMarkdown'
import { ConfirmModal } from '@/components/ui/ConfirmModal'
import { useDispatch, useSelector } from 'react-redux'
import { fetchCourseDetails, fetchCoursesList } from '@/features/courses/coursesSlice'
import { APP_ROUTES } from '@/constants/appRoutes'
import { COURSE_STATUS, COURSE_STATUS_LABELS } from '@/constants/courseStatus'
import { COURSE_SORT_BY } from '@/constants/inputs/getCourses'
import { SORT_DIRECTIONS } from '@/constants/queryParams'
import { GET_COURSES_ACTIVITY } from '@/constants/activities/getCourses'
import { ui } from '@/theme'
import { buildChangedPayload } from '@/pages/courses/courseUpdatePayload'
import { buildCourseCreatePayload, COURSE_CREATE_INITIAL_FORM, isCourseCreateFormValid } from '@/pages/courses/courseCreatePayload'
import { createLessonDeleteConfirmation } from '@/pages/courses/courseLessonDeleteConfirmation'
import { canLoadSelectedLessonDetail } from '@/pages/courses/courseLessonSelection'

export function CourseDetailPage() {
  const { courseId } = useParams()
  const navigate = useNavigate()
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
  const [coursePanelOpen, setCoursePanelOpen] = useState(false)
  const [courseSaving, setCourseSaving] = useState(false)
  const [courseError, setCourseError] = useState(null)
  const [courseForm, setCourseForm] = useState({ name: '', status: '', descriptionMarkdown: '' })
  const [courseInitial, setCourseInitial] = useState(null)
  const [lessonEdit, setLessonEdit] = useState(null)
  const [lessonEditSaving, setLessonEditSaving] = useState(false)
  const [lessonEditError, setLessonEditError] = useState(null)
  const [draggedLessonIndex, setDraggedLessonIndex] = useState(null)
  const [lessonDropIndex, setLessonDropIndex] = useState(null)
  const [reorderingLessons, setReorderingLessons] = useState(false)
  const [selectedLessonId, setSelectedLessonId] = useState(null)
  const [selectedLessonDetail, setSelectedLessonDetail] = useState(null)
  const [selectedLessonLoading, setSelectedLessonLoading] = useState(false)
  const [lessonMediaOpen, setLessonMediaOpen] = useState(false)
  const [lessonMediaSaving, setLessonMediaSaving] = useState(false)
  const [lessonMediaError, setLessonMediaError] = useState(null)
  const [previewMedia, setPreviewMedia] = useState(null)
  const [confirm, setConfirm] = useState(null)
  const [deleting, setDeleting] = useState(false)
  useEffect(() => { dispatch(fetchCourseDetails(courseId)) }, [dispatch, courseId])
  useEffect(() => { setSelectedGalleryIndex(0) }, [courseId, detail.data?.gallery?.length])
  useEffect(() => {
    const firstLessonId = detail.data?.lessons?.[0]?.id ?? null
    setSelectedLessonId((current) => detail.data?.lessons?.some((lesson) => lesson.id === current) ? current : firstLessonId)
  }, [detail.data?.lessons])
  useEffect(() => {
    if (!canLoadSelectedLessonDetail(detail.data, courseId, selectedLessonId)) {
      setSelectedLessonDetail(null)
      setSelectedLessonLoading(false)
      return undefined
    }
    let active = true
    setSelectedLessonLoading(true)
    setLessonMediaError(null)
    fetchCourseLessonDetailRequest(courseId, selectedLessonId)
      .then(({ data }) => { if (active) setSelectedLessonDetail({ ...data, contentMarkdown: <RenderedMarkdown html={data.contentHtml} emptyLabel="Lesson chưa có nội dung." /> }) })
      .catch((error) => {
        if (active) {
          setSelectedLessonDetail(null)
          setLessonMediaError(error?.message ?? 'Không thể tải nội dung lesson.')
        }
      })
      .finally(() => { if (active) setSelectedLessonLoading(false) })
    return () => { active = false }
  }, [courseId, detail.data, selectedLessonId])

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
  const openCourseEditor = () => {
    const snapshot = { name: course.name ?? '', status: course.status ?? '', descriptionMarkdown: course.descriptionMarkdown ?? '' }
    setCourseInitial(snapshot)
    setCourseForm(snapshot)
    setCourseError(null)
    setCoursePanelOpen(true)
  }
  const submitCourseUpdate = async (event) => {
    event.preventDefault()
    const payload = buildChangedPayload(courseInitial, courseForm)
    if (!Object.keys(payload).length) return setCoursePanelOpen(false)
    setCourseSaving(true)
    setCourseError(null)
    try {
      await updateCourseRequest(courseId, payload)
      setCoursePanelOpen(false)
      dispatch(fetchCourseDetails(courseId))
    } catch (error) {
      setCourseError(error?.message ?? 'Không thể cập nhật khóa học.')
    } finally {
      setCourseSaving(false)
    }
  }
  const deleteCourse = async () => {
    setDeleting(true)
    try {
      await deleteCourseRequest(courseId)
      navigate(APP_ROUTES.courses)
    } catch (error) {
      setCourseError(error?.message ?? 'Không thể xóa khóa học.')
      setConfirm(null)
    } finally { setDeleting(false) }
  }
  const openLessonEditor = async (lessonId) => {
    setLessonEditError(null)
    setLessonEdit({ loading: true })
    try {
      const { data } = await fetchCourseLessonDetailRequest(courseId, lessonId)
      const snapshot = { title: data.title ?? '', contentMarkdown: data.contentMarkdown ?? '' }
      setLessonEdit({ loading: false, id: lessonId, initial: snapshot, form: snapshot })
    } catch (error) {
      setLessonEdit({ loading: false })
      setLessonEditError(error?.message ?? 'Không thể tải chi tiết lesson.')
    }
  }
  const submitLessonUpdate = async (event) => {
    event.preventDefault()
    const payload = buildChangedPayload(lessonEdit.initial, lessonEdit.form)
    if (!Object.keys(payload).length) return setLessonEdit(null)
    setLessonEditSaving(true)
    setLessonEditError(null)
    try {
      await updateCourseLessonRequest(courseId, lessonEdit.id, payload)
      setLessonEdit(null)
      dispatch(fetchCourseDetails(courseId))
      const { data } = await fetchCourseLessonDetailRequest(courseId, lessonEdit.id)
      setSelectedLessonDetail({ ...data, contentMarkdown: <RenderedMarkdown html={data.contentHtml} emptyLabel="Lesson chưa có nội dung." /> })
    } catch (error) {
      setLessonEditError(error?.message ?? 'Không thể cập nhật lesson.')
    } finally {
      setLessonEditSaving(false)
    }
  }
  const deleteLesson = async (lessonId) => {
    setDeleting(true)
    try {
      await deleteCourseLessonRequest(courseId, lessonId)
      setSelectedLessonId(null)
      setSelectedLessonDetail(null)
      setConfirm(null)
      dispatch(fetchCourseDetails(courseId))
    } catch (error) {
      setLessonMediaError(error?.message ?? 'Không thể xóa lesson.')
      setConfirm(null)
    } finally { setDeleting(false) }
  }
  const reorderLessons = async (targetIndex) => {
    if (draggedLessonIndex === null || draggedLessonIndex === targetIndex) return
    const lessonIds = course.lessons.map((lesson) => lesson.id)
    const [draggedId] = lessonIds.splice(draggedLessonIndex, 1)
    lessonIds.splice(targetIndex, 0, draggedId)
    setReorderingLessons(true)
    try {
      await reorderCourseLessonsRequest(courseId, lessonIds)
      dispatch(fetchCourseDetails(courseId))
    } catch (error) {
      setLessonMediaError(error?.message ?? 'Không thể đổi thứ tự lesson.')
    } finally {
      setDraggedLessonIndex(null)
      setReorderingLessons(false)
    }
  }
  const attachLessonMedia = async (selected) => {
    if (!selectedLessonId) return
    setLessonMediaSaving(true)
    setLessonMediaError(null)
    try {
      const currentCount = selectedLessonDetail?.attachments?.length ?? 0
      await createMediaUsagesBatchRequest(selected.map((media, index) => ({
        mediaId: media.id,
        ownerService: 'COURSE',
        ownerType: 'LESSON_ATTACHMENT',
        ownerId: selectedLessonId,
        usageType: 'ATTACHMENT',
        displayOrder: currentCount + index,
      })))
      setLessonMediaOpen(false)
      const { data } = await fetchCourseLessonDetailRequest(courseId, selectedLessonId)
      setSelectedLessonDetail({ ...data, contentMarkdown: <RenderedMarkdown html={data.contentHtml} emptyLabel="Lesson chưa có nội dung." /> })
    } catch (error) {
      setLessonMediaError(error?.message ?? 'Không thể gắn media vào lesson.')
    } finally {
      setLessonMediaSaving(false)
    }
  }
  const removeLessonMedia = async (usageId) => {
    setLessonMediaSaving(true)
    setLessonMediaError(null)
    try {
      await removeMediaUsageRequest(usageId)
      const { data } = await fetchCourseLessonDetailRequest(courseId, selectedLessonId)
      setSelectedLessonDetail({ ...data, contentMarkdown: <RenderedMarkdown html={data.contentHtml} emptyLabel="Lesson chưa có nội dung." /> })
    } catch (error) {
      setLessonMediaError(error?.message ?? 'Không thể gỡ media khỏi lesson.')
    } finally {
      setLessonMediaSaving(false)
    }
  }
  const lessonMediaIcon = (mediaType) => ({ IMAGE: Image, VIDEO: Video, AUDIO: Music2, DOCUMENT: FileText }[mediaType] ?? File)
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
                  <div className="max-h-72 bg-surface-muted">
                    {course.thumbnail?.contentUrl ? <MediaImagePreview contentUrl={course.thumbnail.contentUrl} alt={course.name} /> : <button type="button" className={`flex h-41.5 flex-1 w-full flex-col items-center justify-center gap-2 ${ui.body}`} onClick={() => openMedia('thumbnail')}><Icon icon={ImagePlus} size={28} /><span className="text-[12px]">Thêm thumbnail</span></button>}
                  </div>
                  {course.thumbnail ? <button type="button" aria-label="Gỡ thumbnail" disabled={savingMedia} onClick={() => void removeCourseThumbnail()} className="absolute right-2 top-2 rounded-full bg-black/65 p-2 text-white"><Icon icon={Trash2} size={15} /></button> : null}
                  {course.thumbnail ? <button type="button" aria-label="Gỡ thumbnail khi hover" disabled={savingMedia} onClick={() => void removeCourseThumbnail()} className="absolute left-1/2 top-1/2 -translate-x-1/2 -translate-y-1/2 rounded-full bg-black/70 p-3 text-white opacity-0 transition group-hover:opacity-100"><Icon icon={Trash2} size={21} /></button> : null}
                </div>
                <div className="min-w-0">
                  <p className={`text-[11px] font-medium tracking-[0.2em] uppercase ${ui.eyebrow}`}>Course detail</p>
                  <h1 className={`mt-2 max-w-3xl font-display text-[30px] font-semibold leading-tight ${ui.title}`}>{course.name}</h1>
                  <div className="mt-3 flex flex-wrap items-center gap-3"><Dropdown value={course.status} disabled={courseSaving} options={Object.values(COURSE_STATUS).map((value) => ({ value, label: COURSE_STATUS_LABELS[value] }))} triggerClassName={`${course.status === COURSE_STATUS.draft ? ui.statusDraft : course.status === COURSE_STATUS.published ? ui.statusPublished : ui.statusArchived} h-8 min-w-[150px] px-3 text-[12px] font-medium`} onChange={async (status) => { if (status === course.status) return; setCourseSaving(true); try { await updateCourseRequest(courseId, { status }); dispatch(fetchCourseDetails(courseId)) } catch (error) { setCourseError(error?.message ?? 'Không thể cập nhật trạng thái khóa học.') } finally { setCourseSaving(false) } }} /><p className={`text-[14px] ${ui.body}`}>Tạo ngày {new Date(course.createdAtUtc).toLocaleDateString('vi-VN')}</p><Button type="button" size="sm" variant="ghost" onClick={openCourseEditor}><Icon icon={Pencil} size={15} />Chỉnh sửa</Button><Button type="button" size="sm" variant="danger" onClick={() => setConfirm({ kind: 'course', title: 'Xóa khóa học?', description: `Khóa học “${course.name}”, toàn bộ lesson và media usage liên quan sẽ bị xóa.` })}><Icon icon={Trash2} size={15} />Xóa</Button></div>
                  <div className="mt-5 max-w-2xl"><RenderedMarkdown html={course.descriptionHtml} emptyLabel="Chưa có mô tả." /></div>
                </div>
              </section>

              <section className="mt-10">
                <div className="flex items-end justify-between gap-4"><div><p className={`text-[11px] font-medium tracking-[0.2em] uppercase ${ui.eyebrow}`}>Media library</p><h2 className={`mt-1 font-display text-[22px] font-semibold ${ui.title}`}>Gallery khóa học</h2><p className={`mt-1 text-[12px] ${ui.caption}`}>Kéo thumbnail và thả vào ô sáng để đổi vị trí.</p></div><Button type="button" disabled={savingMedia} onClick={() => openMedia('gallery')}><Icon icon={ImagePlus} />Thêm ảnh</Button></div>
                <div className="mt-4 grid gap-4 lg:grid-cols-[190px_minmax(0,1fr)]">
                  <div className="flex gap-2 overflow-x-auto pb-2 lg:max-h-[390px] lg:flex-col lg:overflow-y-auto lg:pr-2">
                    {gallery.length ? gallery.map((media, index) => <div key={media.usageId} draggable={!savingMedia} onDragStart={() => setDraggedGalleryIndex(index)} onDragEnd={() => setDraggedGalleryIndex(null)} onDragOver={(event) => event.preventDefault()} onDrop={() => dropGalleryMedia(index)} onClick={() => setSelectedGalleryIndex(index)} className={`group relative shrink-0 cursor-grab overflow-hidden rounded-md border transition ${draggedGalleryIndex === index ? 'scale-95 border-accent opacity-50 ring-2 ring-accent/40' : selectedGallery?.usageId === media.usageId ? 'border-accent ring-2 ring-accent/30' : 'border-line'} bg-surface-muted active:cursor-grabbing`}><div className="lg:aspect-video"><MediaImagePreview contentUrl={media.thumbnailUrl || media.contentUrl} alt="" /></div><span className="absolute left-2 top-2 rounded bg-black/55 px-1.5 py-0.5 text-[10px] text-white">#{index + 1}</span><button type="button" aria-label="Gỡ ảnh khỏi gallery" disabled={savingMedia} onClick={(event) => { event.stopPropagation(); void removeGalleryMedia(media.usageId) }} className="absolute right-1.5 top-1.5 rounded-full bg-black/60 p-1 text-white opacity-100 transition group-hover:scale-105"><Icon icon={Trash2} size={13} /></button><button type="button" aria-label="Xóa ảnh đang hover" disabled={savingMedia} onClick={(event) => { event.stopPropagation(); void removeGalleryMedia(media.usageId) }} className="absolute left-1/2 top-1/2 -translate-x-1/2 -translate-y-1/2 rounded-full bg-black/65 p-2 text-white opacity-0 transition group-hover:opacity-100"><Icon icon={Trash2} size={17} /></button>{draggedGalleryIndex !== null && draggedGalleryIndex !== index ? <span className="pointer-events-none absolute inset-x-2 bottom-2 rounded bg-accent px-2 py-1 text-center text-[10px] font-medium text-white">Thả để đặt tại đây</span> : null}</div>) : <button type="button" onClick={() => openMedia('gallery')} className={`flex h-[96px] w-32 shrink-0 flex-col items-center justify-center gap-2 rounded-md border border-dashed ${ui.body} lg:w-full`}><Icon icon={ImagePlus} size={25} /><span className="text-[11px]">Tải ảnh lên</span></button>}
                  </div>
                  <div className={`min-h-[280px] overflow-hidden rounded-lg ${ui.card}`}>
                    {selectedGallery ? <a href={selectedGallery.contentUrl} target="_blank" rel="noreferrer" className="block h-full min-h-[280px] bg-surface-muted"><MediaImagePreview contentUrl={selectedGallery.contentUrl} alt={course.name} /></a> : <div className={`flex min-h-[280px] items-center justify-center text-[13px] ${ui.body}`}>Chọn ảnh gallery để xem media chính</div>}
                  </div>
                </div>
                {mediaError ? <p className={`mt-3 rounded-md px-3 py-2 text-[12px] ${ui.badgeDanger}`}>{mediaError}</p> : null}
              </section>

              <section className="mt-10 pb-8"><div className="flex items-end justify-between gap-4"><div><p className={`text-[11px] font-medium tracking-[0.2em] uppercase ${ui.eyebrow}`}>Course content</p><h2 className={`mt-1 font-display text-[22px] font-semibold ${ui.title}`}>Lessons</h2><p className={`mt-1 text-[12px] ${ui.caption}`}>Chọn lesson để xem nội dung; kéo biểu tượng chấm để đổi thứ tự.</p></div><Button type="button" onClick={() => { setLessonError(null); setLessonPanelOpen(true) }}><Icon icon={Plus} />Thêm lesson</Button></div><div className="mt-4 grid gap-4 lg:grid-cols-[280px_minmax(0,1fr)]"><aside className={`overflow-hidden rounded-lg ${ui.card}`} aria-label="Danh sách lesson">{course.lessons.map((lesson, index) => <div key={lesson.id} onDragOver={(event) => event.preventDefault()} onDrop={() => void reorderLessons(index)} className={`flex items-center gap-1 border-b border-line last:border-b-0 ${selectedLessonId === lesson.id ? 'bg-accent-soft' : ''}`}><button type="button" draggable={!reorderingLessons} aria-label={`Kéo ${lesson.title} để đổi thứ tự`} onDragStart={() => setDraggedLessonIndex(index)} onDragEnd={() => setDraggedLessonIndex(null)} className={`cursor-grab p-3 ${ui.caption} active:cursor-grabbing ${draggedLessonIndex === index ? 'opacity-40' : ''}`}><Icon icon={GripVertical} size={17} /></button><button type="button" onClick={() => setSelectedLessonId(lesson.id)} className={`min-w-0 flex-1 px-1 py-3 text-left ${selectedLessonId === lesson.id ? ui.title : ui.body}`}><span className={`mr-2 font-mono text-[11px] ${ui.caption}`}>{String(index + 1).padStart(2, '0')}</span><span className="text-[13px] font-medium">{lesson.title}</span></button></div>)}{!course.lessons.length ? <p className={`p-4 text-[13px] ${ui.body}`}>Chưa có lesson.</p> : null}</aside><div className={`min-h-[360px] rounded-lg p-5 ${ui.card}`}>{selectedLessonLoading ? <LoadingState label="Đang tải nội dung lesson..." /> : selectedLessonDetail ? <><div className="flex items-start justify-between gap-3"><div><p className={`text-[11px] font-medium tracking-[0.18em] uppercase ${ui.eyebrow}`}>Lesson {String(course.lessons.findIndex((lesson) => lesson.id === selectedLessonId) + 1).padStart(2, '0')}</p><h3 className={`mt-1 font-display text-[20px] font-semibold ${ui.title}`}>{selectedLessonDetail.title}</h3></div><div className="flex shrink-0 items-center gap-2"><Button type="button" size="sm" variant="ghost" onClick={() => void openLessonEditor(selectedLessonDetail.id)}><Icon icon={Pencil} size={15} />Sửa</Button><Button type="button" size="sm" variant="danger" onClick={() => setConfirm(createLessonDeleteConfirmation(selectedLessonDetail))}><Icon icon={Trash2} size={15} />Xóa lesson</Button></div></div><div className={`mt-5 whitespace-pre-wrap text-[14px] leading-6 ${ui.body}`}>{selectedLessonDetail.contentMarkdown || 'Lesson chưa có nội dung.'}</div><div className={`mt-7 pt-5 ${ui.hairlineT}`}><div className="flex items-center justify-between gap-3"><div><p className={`text-[11px] font-medium tracking-[0.18em] uppercase ${ui.eyebrow}`}>Media đính kèm</p><p className={`mt-1 text-[12px] ${ui.caption}`}>Hỗ trợ ảnh, video, audio, tài liệu và tệp khác.</p></div><Button type="button" size="sm" disabled={lessonMediaSaving} onClick={() => setLessonMediaOpen(true)}><Icon icon={ImagePlus} size={15} />Thêm tài liệu</Button></div>{selectedLessonDetail.attachments?.length ? <div className="mt-4 grid gap-3 sm:grid-cols-2 xl:grid-cols-3">{selectedLessonDetail.attachments.map((media) => { const MediaIcon = lessonMediaIcon(media.mediaType); const previewUrl = media.thumbnailUrl || (media.mediaType === 'IMAGE' ? media.contentUrl : null); return <article key={media.usageId} className={`group relative overflow-hidden rounded-md ${ui.choiceIdle}`}><button type="button" className="w-full cursor-pointer text-left" aria-label={`Xem trước ${media.originalFileName}`} onClick={() => setPreviewMedia(media)}><div className="aspect-video bg-surface-muted">{previewUrl ? <MediaImagePreview contentUrl={previewUrl} alt={media.originalFileName} /> : <div className={`flex h-full flex-col items-center justify-center gap-2 ${ui.caption}`}><Icon icon={MediaIcon} size={30} /><span className="text-[11px]">{media.mediaType}</span></div>}</div><div className="min-w-0 p-3"><p className={`truncate text-[12px] font-medium ${ui.title}`}>{media.originalFileName}</p><p className={`mt-1 text-[11px] ${ui.caption}`}>{media.mediaType} · {media.contentType}</p></div></button><button type="button" aria-label={`Gỡ ${media.originalFileName}`} disabled={lessonMediaSaving} onClick={(event) => { event.stopPropagation(); void removeLessonMedia(media.usageId) }} className={`absolute right-2 top-2 rounded-full p-1.5 opacity-0 transition group-hover:opacity-100 focus:opacity-100 ${ui.mediaAction}`}><Icon icon={Trash2} size={14} /></button></article> })}</div> : <p className={`mt-4 rounded-md border border-dashed p-4 text-[13px] ${ui.body}`}>Chưa có media đính kèm.</p>}{lessonMediaError ? <p className={`mt-3 rounded-md px-3 py-2 text-[12px] ${ui.badgeDanger}`}>{lessonMediaError}</p> : null}</div></> : <EmptyState title="Chọn một lesson" description="Nội dung và media đính kèm sẽ hiển thị tại đây." />}</div></div></section>
            </div>
      <RightPanel open={lessonPanelOpen} title="Thêm lesson" description={`Lesson mới sẽ được thêm vào cuối khóa học.`} onClose={() => { if (!lessonSaving) setLessonPanelOpen(false) }} footer={<div className="flex justify-end gap-2"><Button type="button" variant="ghost" disabled={lessonSaving} onClick={() => setLessonPanelOpen(false)}>Hủy</Button><Button type="submit" form="create-lesson-form" disabled={lessonSaving || !lessonForm.title.trim()}>Lưu lesson</Button></div>}>
        <form id="create-lesson-form" className="space-y-4" onSubmit={submitLesson}>
          <label className={`block text-[13px] ${ui.body}`}>Tiêu đề lesson<input className="mt-1 w-full rounded-md border border-line bg-transparent px-3 py-2 text-[14px]" maxLength={200} value={lessonForm.title} onChange={(event) => setLessonForm((current) => ({ ...current, title: event.target.value }))} placeholder="Ví dụ: Tổng quan về hệ thống" /></label>
          <MarkdownEditor value={lessonForm.contentMarkdown} onChange={(contentMarkdown) => setLessonForm((current) => ({ ...current, contentMarkdown }))} placeholder="Nội dung lesson (không bắt buộc)" />
          {lessonError ? <p className={`rounded-md px-3 py-2 text-[12px] ${ui.badgeDanger}`}>{lessonError}</p> : null}
        </form>
      </RightPanel>
      <RightPanel open={coursePanelOpen} title="Chỉnh sửa khóa học" description="Chỉ các trường đã thay đổi mới được gửi." onClose={() => { if (!courseSaving) setCoursePanelOpen(false) }} footer={<div className="flex justify-end gap-2"><Button type="button" variant="ghost" disabled={courseSaving} onClick={() => setCoursePanelOpen(false)}>Hủy</Button><Button type="submit" form="update-course-form" disabled={courseSaving}>Lưu thay đổi</Button></div>}>
        <form id="update-course-form" className="space-y-4" onSubmit={submitCourseUpdate}>
          <label className={`block text-[13px] ${ui.body}`}>Tên khóa học<input className={`${ui.control} mt-1 w-full py-2`} maxLength={200} value={courseForm.name} onChange={(event) => setCourseForm((current) => ({ ...current, name: event.target.value }))} /></label>
          <Dropdown label="Trạng thái" value={courseForm.status} disabled={courseSaving} options={Object.values(COURSE_STATUS).map((value) => ({ value, label: COURSE_STATUS_LABELS[value] }))} onChange={(status) => setCourseForm((current) => ({ ...current, status }))} />
          <MarkdownEditor id="course-description-markdown" value={courseForm.descriptionMarkdown} disabled={courseSaving} onChange={(descriptionMarkdown) => setCourseForm((current) => ({ ...current, descriptionMarkdown }))} placeholder="Mô tả Markdown (để trống để xóa)" />
          {courseError ? <p className={`rounded-md px-3 py-2 text-[12px] ${ui.badgeDanger}`}>{courseError}</p> : null}
        </form>
      </RightPanel>
      <RightPanel open={lessonEdit !== null} title="Chỉnh sửa lesson" description={lessonEdit?.loading ? 'Đang tải chi tiết lesson...' : 'Chỉ các trường đã thay đổi mới được gửi.'} onClose={() => { if (!lessonEditSaving) setLessonEdit(null) }} footer={lessonEdit?.loading ? null : <div className="flex justify-end gap-2"><Button type="button" variant="danger" disabled={lessonEditSaving} onClick={() => setConfirm(createLessonDeleteConfirmation({ id: lessonEdit.id, title: lessonEdit.form.title }))}>Xóa</Button><Button type="button" variant="ghost" disabled={lessonEditSaving} onClick={() => setLessonEdit(null)}>Hủy</Button><Button type="submit" form="update-lesson-form" disabled={lessonEditSaving}>Lưu thay đổi</Button></div>}>
        {lessonEdit?.loading ? <LoadingState label="Đang tải lesson..." /> : lessonEdit?.form ? <form id="update-lesson-form" className="space-y-4" onSubmit={submitLessonUpdate}>
          <label className={`block text-[13px] ${ui.body}`}>Tiêu đề lesson<input className={`${ui.control} mt-1 w-full py-2`} maxLength={200} value={lessonEdit.form.title} onChange={(event) => setLessonEdit((current) => ({ ...current, form: { ...current.form, title: event.target.value } }))} /></label>
          <MarkdownEditor id="lesson-content-markdown" value={lessonEdit.form.contentMarkdown} disabled={lessonEditSaving} onChange={(contentMarkdown) => setLessonEdit((current) => ({ ...current, form: { ...current.form, contentMarkdown } }))} placeholder="Nội dung Markdown (để trống để xóa)" />
          {lessonEditError ? <p className={`rounded-md px-3 py-2 text-[12px] ${ui.badgeDanger}`}>{lessonEditError}</p> : null}
        </form> : null}
      </RightPanel>
      <MediaLibraryModal
        open={mediaMode !== null}
        selectionMode={mediaMode === 'gallery' ? 'multiple' : 'single'}
        initialSelection={mediaMode === 'gallery' ? course.gallery?.map((media) => ({ id: media.mediaId })) : []}
        onClose={() => { if (!savingMedia) setMediaMode(null) }}
        onConfirm={saveMedia}
      />
      <MediaLibraryModal
        open={lessonMediaOpen}
        selectionMode="multiple"
        initialSelection={selectedLessonDetail?.attachments?.map((media) => ({ id: media.mediaId })) ?? []}
        onClose={() => { if (!lessonMediaSaving) setLessonMediaOpen(false) }}
        onConfirm={attachLessonMedia}
      />
      <MediaPreviewModal
        open={previewMedia !== null}
        media={previewMedia}
        onClose={() => setPreviewMedia(null)}
      />
      <ConfirmModal open={confirm !== null} mode="delete" title={confirm?.title} description={confirm?.description} confirmLabel="Xóa" busy={deleting} onClose={() => setConfirm(null)} onConfirm={() => { if (confirm?.kind === 'course') void deleteCourse(); if (confirm?.kind === 'lesson') void deleteLesson(confirm.lessonId) }} />
    </main>
  )
}

export function CoursesPage() {
  const dispatch = useDispatch()
  const list = useSelector((state) => state.courses.list)
  const loaded = useRef(false)
  const [coursePanelOpen, setCoursePanelOpen] = useState(false)
  const [courseSaving, setCourseSaving] = useState(false)
  const [courseError, setCourseError] = useState(null)
  const [courseForm, setCourseForm] = useState(COURSE_CREATE_INITIAL_FORM)

  useEffect(() => {
    if (!loaded.current) {
      loaded.current = true
      dispatch(fetchCoursesList(list.query))
    }
  }, [dispatch, list.query])

  const load = (patch) => dispatch(fetchCoursesList({ ...list.query, ...patch }))
  const openCourseCreator = () => {
    setCourseForm(COURSE_CREATE_INITIAL_FORM)
    setCourseError(null)
    setCoursePanelOpen(true)
  }
  const submitCourseCreate = async (event) => {
    event.preventDefault()
    if (!isCourseCreateFormValid(courseForm)) return
    setCourseSaving(true)
    setCourseError(null)
    try {
      await createCourseRequest(buildCourseCreatePayload(courseForm))
      setCoursePanelOpen(false)
      setCourseForm(COURSE_CREATE_INITIAL_FORM)
      dispatch(fetchCoursesList({ ...list.query, page: 1 }))
    } catch (error) {
      setCourseError(error?.message ?? 'Không thể tạo khóa học.')
    } finally {
      setCourseSaving(false)
    }
  }

  return (
    <>
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
          <div className="mb-4 flex justify-end"><Button type="button" onClick={openCourseCreator}><Icon icon={Plus} />Tạo khóa học</Button></div>
          {list.data.length ? <div className="grid gap-4 sm:grid-cols-2 xl:grid-cols-3">{list.data.map((course) => <Link key={course.id} to={APP_ROUTES.courseDetails.replace(':courseId', course.id)} className={`overflow-hidden rounded-lg ${ui.card}`}><div className="aspect-video bg-surface-muted">{course.thumbnailUrl ? <MediaImagePreview contentUrl={course.thumbnailUrl} alt={course.name} /> : <div className={`flex h-full items-center justify-center text-[12px] ${ui.caption}`}>Chưa có thumbnail</div>}</div><div className="p-4"><h2 className={`truncate text-[15px] font-medium ${ui.title}`}>{course.name}</h2><p className={`mt-2 text-[12px] ${ui.body}`}>{course.status}</p></div></Link>)}</div> : null}
        </OutputPanel>
      }
      />
      <RightPanel open={coursePanelOpen} title="Tạo khóa học" description="Khóa học mới sẽ được lưu ở trạng thái Bản nháp." onClose={() => { if (!courseSaving) setCoursePanelOpen(false) }} footer={<div className="flex justify-end gap-2"><Button type="button" variant="ghost" disabled={courseSaving} onClick={() => setCoursePanelOpen(false)}>Hủy</Button><Button type="submit" form="create-course-form" disabled={courseSaving || !isCourseCreateFormValid(courseForm)}>Tạo khóa học</Button></div>}>
        <form id="create-course-form" className="space-y-4" onSubmit={submitCourseCreate}>
          <label className={`block text-[13px] ${ui.body}`}>Tên khóa học<input className={`${ui.control} mt-1 w-full py-2`} maxLength={200} disabled={courseSaving} value={courseForm.name} onChange={(event) => setCourseForm((current) => ({ ...current, name: event.target.value }))} placeholder="Ví dụ: Backend Fundamentals" /></label>
          <MarkdownEditor id="create-course-description-markdown" value={courseForm.descriptionMarkdown} disabled={courseSaving} onChange={(descriptionMarkdown) => setCourseForm((current) => ({ ...current, descriptionMarkdown }))} placeholder="Mô tả Markdown (không bắt buộc)" />
          {courseError ? <p className={`rounded-md px-3 py-2 text-[12px] ${ui.badgeDanger}`}>{courseError}</p> : null}
        </form>
      </RightPanel>
    </>
  )
}
