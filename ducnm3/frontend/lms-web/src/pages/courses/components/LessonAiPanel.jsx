import { useState } from 'react'
import { FileText, ListChecks, RotateCw, Sparkles } from 'lucide-react'
import { Button } from '@/components/ui/admin/Button'
import { ConfirmModal } from '@/components/ui/admin/ConfirmModal'
import { Icon } from '@/components/ui/admin/Icon'
import { Spinner } from '@/components/ui/admin/Spinner'
import {
  LESSON_AI_COPY,
  LESSON_AI_DOCUMENT_STATUS,
  LESSON_AI_DOCUMENT_STATUS_LABELS,
  LESSON_AI_ERROR_CODES,
  LESSON_AI_ERROR_HINTS,
  LESSON_AI_JOB_STATUS,
  LESSON_AI_JOB_STATUS_LABELS,
  LESSON_AI_JOB_TYPE,
  LESSON_AI_JOB_TYPE_LABELS,
  LESSON_AI_QUESTION_COUNT,
} from '@/constants/lessonAi'
import { isDocumentSelectable, isJobActive, useLessonAi } from '@/hooks/lessonAi/useLessonAi'
import { adminUi } from '@/theme/admin'

const RECENT_JOBS_SHOWN = 4

const documentTone = {
  [LESSON_AI_DOCUMENT_STATUS.extracted]: adminUi.badgeSuccess,
  [LESSON_AI_DOCUMENT_STATUS.failed]: adminUi.badgeDanger,
  [LESSON_AI_DOCUMENT_STATUS.pending]: adminUi.badgeWarning,
  [LESSON_AI_DOCUMENT_STATUS.extracting]: adminUi.badgeWarning,
}

const jobTone = {
  [LESSON_AI_JOB_STATUS.completed]: adminUi.badgeSuccess,
  [LESSON_AI_JOB_STATUS.failed]: adminUi.badgeDanger,
  [LESSON_AI_JOB_STATUS.pending]: adminUi.badgeWarning,
  [LESSON_AI_JOB_STATUS.running]: adminUi.badgeWarning,
}

function Badge({ tone = adminUi.badgeMuted, children }) {
  return <span className={`inline-flex shrink-0 items-center gap-1 rounded-full px-2 py-0.5 text-[12px] font-medium ${tone}`}>{children}</span>
}

function errorText(code, message) {
  return LESSON_AI_ERROR_HINTS[code] ?? message ?? code
}

function formatTime(value) {
  return value ? new Date(value).toLocaleString('vi-VN') : ''
}

/** Chọn tài liệu đã trích xuất rồi nhờ AI tổng hợp bài học hoặc soạn quiz nháp cho lesson. */
export function LessonAiPanel({ courseId, lessonId, attachmentKey, lessonHasContent, onLessonContentChanged, onOpenQuizzes }) {
  const ai = useLessonAi({ courseId, lessonId, attachmentKey, onContentChanged: onLessonContentChanged })
  const [questionCount, setQuestionCount] = useState(LESSON_AI_QUESTION_COUNT.default)
  const [confirmOverwrite, setConfirmOverwrite] = useState(false)

  const selectable = ai.documents.filter(isDocumentSelectable)
  const hasSelection = ai.selectedIds.length > 0
  const activeTypes = new Set(ai.jobs.filter(isJobActive).map((job) => job.jobType))
  const hasFailed = ai.documents.some((document) => document.status === LESSON_AI_DOCUMENT_STATUS.failed)
  const questionCountValid = Number.isInteger(questionCount)
    && questionCount >= LESSON_AI_QUESTION_COUNT.min
    && questionCount <= LESSON_AI_QUESTION_COUNT.max

  const canSynthesize = hasSelection && !ai.submitting && !activeTypes.has(LESSON_AI_JOB_TYPE.synthesize)
  const canGenerateQuiz = hasSelection && questionCountValid && !ai.submitting && !activeTypes.has(LESSON_AI_JOB_TYPE.quiz)

  const synthesize = async (overwrite) => {
    setConfirmOverwrite(false)
    const result = await ai.startJob(LESSON_AI_JOB_TYPE.synthesize, { overwrite })
    // Nội dung lesson có thể vừa được người khác thêm sau khi trang tải: hỏi xác nhận thay vì ghi đè âm thầm.
    if (!result.ok && result.code === LESSON_AI_ERROR_CODES.contentExists) setConfirmOverwrite(true)
  }

  const onSynthesizeClick = () => {
    if (lessonHasContent) setConfirmOverwrite(true)
    else void synthesize(false)
  }

  const recentJobs = ai.jobs.slice(0, RECENT_JOBS_SHOWN)

  return (
    <section className={`mt-6 rounded-md p-4 ${adminUi.choiceIdle}`} aria-label={LESSON_AI_COPY.title}>
      <div className="flex flex-wrap items-start justify-between gap-3">
        <div>
          <p className={`flex items-center gap-1.5 text-[11px] font-medium tracking-[0.18em] uppercase ${adminUi.eyebrow}`}><Icon icon={Sparkles} size={14} />{LESSON_AI_COPY.title}</p>
          <p className={`mt-1 text-[12px] ${adminUi.caption}`}>{LESSON_AI_COPY.hint}</p>
        </div>
        {hasFailed ? <Button type="button" size="sm" variant="ghost" onClick={() => void ai.retryFailed()}><Icon icon={RotateCw} size={15} />{LESSON_AI_COPY.retryFailed}</Button> : null}
      </div>

      {ai.error ? <p className={`mt-3 rounded-md px-3 py-2 text-[12px] ${adminUi.badgeDanger}`}>{LESSON_AI_COPY.loadFailed} {ai.error.message}</p> : null}

      {ai.loading ? <div className={`mt-4 flex items-center gap-2 text-[13px] ${adminUi.caption}`}><Spinner />Đang kiểm tra tài liệu...</div> : null}

      {!ai.loading && ai.documents.length === 0 && !ai.error ? <p className={`mt-4 text-[13px] ${adminUi.caption}`}>{LESSON_AI_COPY.noAttachments}</p> : null}

      {ai.documents.length > 0 ? (
        <div className="mt-4">
          <div className="flex flex-wrap items-center justify-between gap-2">
            <p className={`text-[13px] font-medium ${adminUi.title}`}>{LESSON_AI_COPY.documentsHeading}</p>
            <div className="flex items-center gap-2">
              <span className={`text-[12px] ${adminUi.caption}`}>{LESSON_AI_COPY.selectedCount(ai.selectedIds.length)}</span>
              <Button type="button" size="sm" variant="ghost" disabled={selectable.length === 0 || ai.selectedIds.length === selectable.length} onClick={ai.selectAllExtracted}>{LESSON_AI_COPY.selectAll}</Button>
              <Button type="button" size="sm" variant="ghost" disabled={!hasSelection} onClick={ai.clearSelection}>{LESSON_AI_COPY.clearSelection}</Button>
            </div>
          </div>
          <ul className="mt-2 grid gap-1.5">
            {ai.documents.map((document) => {
              const selectableDocument = isDocumentSelectable(document)
              const inProgress = document.status === LESSON_AI_DOCUMENT_STATUS.pending || document.status === LESSON_AI_DOCUMENT_STATUS.extracting
              return (
                <li key={document.mediaId} className="flex items-start gap-3 rounded-md px-2 py-1.5">
                  <input
                    id={`lesson-ai-doc-${document.mediaId}`}
                    type="checkbox"
                    className={`mt-0.5 ${adminUi.checkbox}`}
                    checked={ai.selectedIds.includes(document.mediaId)}
                    disabled={!selectableDocument}
                    onChange={() => ai.toggleDocument(document.mediaId)}
                  />
                  <label htmlFor={`lesson-ai-doc-${document.mediaId}`} className={`min-w-0 flex-1 ${selectableDocument ? 'cursor-pointer' : 'cursor-not-allowed'}`}>
                    <span className={`flex items-center gap-1.5 text-[13px] ${adminUi.title}`}><Icon icon={FileText} size={14} /><span className="truncate">{document.fileName}</span></span>
                    {document.errorCode ? <span className={`mt-0.5 block text-[12px] ${adminUi.caption}`}>{errorText(document.errorCode, document.errorMessage)}</span> : null}
                  </label>
                  <Badge tone={documentTone[document.status]}>
                    {inProgress ? <Spinner size={12} /> : null}
                    {LESSON_AI_DOCUMENT_STATUS_LABELS[document.status] ?? document.status}
                  </Badge>
                </li>
              )
            })}
          </ul>

          <div className="mt-4 flex flex-wrap items-end gap-3">
            <Button type="button" disabled={!canSynthesize} onClick={onSynthesizeClick}><Icon icon={FileText} size={15} />{LESSON_AI_COPY.synthesize}</Button>
            <div className="flex items-end gap-2">
              <div className="w-24">
                <label htmlFor="lesson-ai-question-count" className={`mb-1 block text-[12px] ${adminUi.caption}`}>{LESSON_AI_COPY.questionCount}</label>
                <input
                  id="lesson-ai-question-count"
                  type="number"
                  className={adminUi.control}
                  min={LESSON_AI_QUESTION_COUNT.min}
                  max={LESSON_AI_QUESTION_COUNT.max}
                  value={Number.isNaN(questionCount) ? '' : questionCount}
                  onChange={(event) => setQuestionCount(event.target.value === '' ? Number.NaN : Number(event.target.value))}
                />
              </div>
              <Button type="button" disabled={!canGenerateQuiz} onClick={() => void ai.startJob(LESSON_AI_JOB_TYPE.quiz, { questionCount })}><Icon icon={ListChecks} size={15} />{LESSON_AI_COPY.generateQuiz}</Button>
            </div>
          </div>
          {!hasSelection ? <p className={`mt-2 text-[12px] ${adminUi.caption}`}>{LESSON_AI_COPY.needSelection}</p> : null}
        </div>
      ) : null}

      <div className="mt-5">
        <p className={`text-[13px] font-medium ${adminUi.title}`}>{LESSON_AI_COPY.jobsHeading}</p>
        {recentJobs.length === 0 ? <p className={`mt-1 text-[12px] ${adminUi.caption}`}>{LESSON_AI_COPY.noJobs}</p> : (
          <ul className="mt-2 grid gap-2">
            {recentJobs.map((job) => (
              <li key={job.id} className={`rounded-md px-3 py-2 ${adminUi.choiceIdle}`}>
                <div className="flex flex-wrap items-center justify-between gap-2">
                  <span className={`text-[13px] font-medium ${adminUi.title}`}>{LESSON_AI_JOB_TYPE_LABELS[job.jobType] ?? job.jobType}</span>
                  <Badge tone={jobTone[job.status]}>
                    {isJobActive(job) ? <Spinner size={12} /> : null}
                    {LESSON_AI_JOB_STATUS_LABELS[job.status] ?? job.status}
                  </Badge>
                </div>
                <p className={`mt-0.5 text-[12px] ${adminUi.caption}`}>{formatTime(job.createdAtUtc)} · {job.mediaIds.length} tài liệu</p>
                {job.status === LESSON_AI_JOB_STATUS.failed ? <p className={`mt-1 text-[12px] ${adminUi.caption}`}>{errorText(job.errorCode, job.errorMessage)}</p> : null}
                {job.status === LESSON_AI_JOB_STATUS.completed ? (
                  <div className="mt-1 flex flex-wrap items-center gap-2">
                    <span className={`text-[12px] ${adminUi.caption}`}>{LESSON_AI_COPY.reviewNotice}</span>
                    {job.resultQuizId && onOpenQuizzes ? <Button type="button" size="sm" variant="ghost" onClick={() => onOpenQuizzes(lessonId)}><Icon icon={ListChecks} size={14} />{LESSON_AI_COPY.openQuiz}</Button> : null}
                  </div>
                ) : null}
              </li>
            ))}
          </ul>
        )}
      </div>

      <ConfirmModal
        open={confirmOverwrite}
        mode="warning"
        title={LESSON_AI_COPY.overwriteTitle}
        description={LESSON_AI_COPY.overwriteDescription}
        confirmLabel={LESSON_AI_COPY.overwriteConfirm}
        busy={ai.submitting}
        onClose={() => setConfirmOverwrite(false)}
        onConfirm={() => void synthesize(true)}
      />
    </section>
  )
}
