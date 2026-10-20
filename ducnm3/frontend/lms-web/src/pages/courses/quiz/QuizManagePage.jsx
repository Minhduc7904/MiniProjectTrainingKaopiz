import { useCallback, useEffect, useState } from 'react'
import { Link, useParams } from 'react-router-dom'
import { ArrowLeft, ListChecks, Plus } from 'lucide-react'
import { createQuizRequest, deleteQuizRequest, fetchQuizDetailRequest, fetchQuizzesRequest, updateQuizRequest } from '@/api/quizApi'
import { fetchCourseLessonDetailRequest } from '@/api/coursesApi'
import { Button } from '@/components/ui/admin/Button'
import { ConfirmModal } from '@/components/ui/admin/ConfirmModal'
import { EmptyState } from '@/components/ui/admin/EmptyState'
import { Icon } from '@/components/ui/admin/Icon'
import { LoadingState } from '@/components/ui/admin/LoadingState'
import { APP_ROUTES } from '@/constants/appRoutes'
import { QUIZ_STATUS_LABELS } from '@/constants/quiz'
import { adminUi } from '@/theme/admin'
import { QuizDetailView } from './QuizDetailView'
import { QuizEditor } from './QuizEditor'
import { emptyQuiz, fromQuizDetail, toQuizPayload } from './quizForm'

const IDLE_PANEL = { mode: 'idle' }

/** Quản lý quiz của một lesson: danh sách bên trái, xem/soạn/xóa bên phải. */
export function QuizManagePage() {
  const { courseId, lessonId } = useParams()
  const [lesson, setLesson] = useState(null)
  const [quizzes, setQuizzes] = useState([])
  const [loading, setLoading] = useState(true)
  const [loadError, setLoadError] = useState(null)
  const [selectedId, setSelectedId] = useState(null)
  const [panel, setPanel] = useState(IDLE_PANEL)
  const [saving, setSaving] = useState(false)
  const [saveError, setSaveError] = useState(null)
  const [confirmDelete, setConfirmDelete] = useState(false)

  const loadQuizzes = useCallback(async () => {
    try {
      const { data } = await fetchQuizzesRequest(courseId, lessonId)
      setQuizzes(data)
      setLoadError(null)
      return data
    } catch (error) {
      setLoadError(error?.message ?? 'Không thể tải danh sách quiz.')
      return []
    } finally {
      setLoading(false)
    }
  }, [courseId, lessonId])

  const openQuiz = useCallback(async (quizId) => {
    setSelectedId(quizId)
    setSaveError(null)
    setPanel({ mode: 'loading' })
    try {
      const { data } = await fetchQuizDetailRequest(courseId, lessonId, quizId)
      setPanel({ mode: 'view', detail: data })
    } catch (error) {
      setPanel({ mode: 'error', message: error?.message ?? 'Không thể tải chi tiết quiz.' })
    }
  }, [courseId, lessonId])

  useEffect(() => {
    let active = true
    fetchCourseLessonDetailRequest(courseId, lessonId).then(({ data }) => { if (active) setLesson(data) }).catch(() => {})
    void loadQuizzes()
    return () => { active = false }
  }, [courseId, lessonId, loadQuizzes])

  const startCreate = () => { setSelectedId(null); setSaveError(null); setPanel({ mode: 'create', form: emptyQuiz() }) }
  const startEdit = () => { setSaveError(null); setPanel({ mode: 'edit', detail: panel.detail, form: fromQuizDetail(panel.detail) }) }
  const cancelEdit = () => { setSaveError(null); if (panel.mode === 'edit') setPanel({ mode: 'view', detail: panel.detail }); else setPanel(IDLE_PANEL) }

  const save = async () => {
    const locked = panel.mode === 'edit' && panel.detail.hasAttempts
    const payload = toQuizPayload(panel.form, { includeQuestions: !locked })
    setSaving(true)
    setSaveError(null)
    try {
      const { data } = panel.mode === 'create'
        ? await createQuizRequest(courseId, lessonId, payload)
        : await updateQuizRequest(courseId, lessonId, panel.detail.id, payload)
      setSelectedId(data.id)
      setPanel({ mode: 'view', detail: data })
      await loadQuizzes()
    } catch (error) {
      setSaveError(error?.message ?? 'Không thể lưu quiz.')
    } finally {
      setSaving(false)
    }
  }

  const remove = async () => {
    setSaving(true)
    try {
      await deleteQuizRequest(courseId, lessonId, panel.detail.id)
      setConfirmDelete(false)
      setSelectedId(null)
      setPanel(IDLE_PANEL)
      await loadQuizzes()
    } catch (error) {
      setConfirmDelete(false)
      setPanel({ mode: 'error', message: error?.message ?? 'Không thể xóa quiz.' })
    } finally {
      setSaving(false)
    }
  }

  return (
    <main className={`h-screen min-h-screen overflow-y-auto px-5 py-6 md:px-8 md:py-8 ${adminUi.page}`}>
      <div className="mx-auto max-w-6xl">
        <Link to={APP_ROUTES.courseDetails.replace(':courseId', courseId)} className={`inline-flex items-center gap-2 text-[13px] ${adminUi.body}`}><Icon icon={ArrowLeft} />Quay lại khóa học</Link>
        <header className="mt-4">
          <p className={`text-[11px] font-medium tracking-[0.2em] uppercase ${adminUi.eyebrow}`}>Quiz trắc nghiệm</p>
          <h2 className={`mt-1 font-display text-[22px] font-semibold ${adminUi.title}`}>{lesson?.title ?? 'Lesson'}</h2>
          <p className={`mt-1 text-[13px] ${adminUi.body}`}>Mỗi lesson có thể có nhiều quiz; mỗi quiz gồm nhiều câu hỏi chọn 1 hoặc chọn nhiều, mỗi câu có điểm riêng.</p>
        </header>

        <section className="mt-6 grid items-start gap-6 lg:grid-cols-[minmax(240px,300px)_minmax(0,1fr)]">
          <aside className={`rounded-lg p-3 ${adminUi.card}`} aria-label="Danh sách quiz">
            <Button type="button" size="sm" onClick={startCreate}><Icon icon={Plus} size={15} />Tạo quiz</Button>
            {loading ? <div className="mt-4"><LoadingState label="Đang tải quiz..." /></div> : null}
            {loadError ? <p role="alert" className={`mt-3 text-[13px] ${adminUi.dangerText}`}>{loadError}</p> : null}
            {!loading && !loadError && quizzes.length === 0 ? <p className={`mt-4 text-[13px] ${adminUi.body}`}>Lesson này chưa có quiz.</p> : null}
            <ul className="mt-3 grid gap-2">
              {quizzes.map((quiz) => (
                <li key={quiz.id}>
                  <button type="button" aria-pressed={selectedId === quiz.id} onClick={() => void openQuiz(quiz.id)} className={`w-full cursor-pointer rounded-md px-3 py-2 text-left ${selectedId === quiz.id ? adminUi.choiceActive : adminUi.choiceIdle}`}>
                    <span className={`block truncate text-[14px] font-medium ${adminUi.title}`}>{quiz.title}</span>
                    <span className={`mt-0.5 flex flex-wrap gap-x-2 text-[12px] ${adminUi.caption}`}>
                      <span>{QUIZ_STATUS_LABELS[quiz.status]}</span><span>{quiz.questionCount} câu</span><span>{quiz.totalPoints} điểm</span>
                    </span>
                  </button>
                </li>
              ))}
            </ul>
          </aside>

          <div className={`min-w-0 rounded-lg p-5 ${adminUi.card}`} aria-live="polite">
            {panel.mode === 'idle' ? <EmptyState title="Chọn hoặc tạo quiz" description="Chọn một quiz ở bên trái để xem, hoặc bấm “Tạo quiz” để soạn mới." /> : null}
            {panel.mode === 'loading' ? <LoadingState label="Đang tải chi tiết quiz..." /> : null}
            {panel.mode === 'error' ? <p role="alert" className={`text-[13px] ${adminUi.dangerText}`}>{panel.message}</p> : null}
            {panel.mode === 'view' ? <QuizDetailView quiz={panel.detail} onEdit={startEdit} onDelete={() => setConfirmDelete(true)} /> : null}
            {panel.mode === 'create' || panel.mode === 'edit' ? (
              <>
                <h3 className={`mb-4 flex items-center gap-2 font-display text-[18px] font-semibold ${adminUi.title}`}><Icon icon={ListChecks} size={18} />{panel.mode === 'create' ? 'Tạo quiz mới' : 'Sửa quiz'}</h3>
                <QuizEditor
                  form={panel.form}
                  structureLocked={panel.mode === 'edit' && panel.detail.hasAttempts}
                  saving={saving}
                  serverError={saveError}
                  submitLabel={panel.mode === 'create' ? 'Tạo quiz' : 'Lưu thay đổi'}
                  onChange={(form) => setPanel((current) => ({ ...current, form }))}
                  onSubmit={() => void save()}
                  onCancel={cancelEdit}
                />
              </>
            ) : null}
          </div>
        </section>
      </div>

      <ConfirmModal open={confirmDelete} mode="delete" title="Xóa quiz?" description={`“${panel.detail?.title ?? ''}” cùng toàn bộ câu hỏi và bài làm của học viên sẽ bị xóa vĩnh viễn.`} confirmLabel="Xóa quiz" busy={saving} onClose={() => setConfirmDelete(false)} onConfirm={() => void remove()} />
    </main>
  )
}
