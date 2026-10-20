import { useEffect, useRef } from 'react'
import { AlertTriangle, Send } from 'lucide-react'
import { studentUi } from '@/theme/student'

/** Hộp xác nhận nộp bài: cho biết số câu đã trả lời và liệt kê câu bỏ trống để nhảy tới sửa. */
export function QuizSubmitDialog({ open, total, answered, blankIndexes, submitting, error, onGo, onCancel, onConfirm }) {
  const dialogRef = useRef(null)

  useEffect(() => {
    const dialog = dialogRef.current
    if (!dialog) return
    if (open && !dialog.open) { if (typeof dialog.showModal === 'function') dialog.showModal(); else dialog.setAttribute('open', '') }
    if (!open && dialog.open) { if (typeof dialog.close === 'function') dialog.close(); else dialog.removeAttribute('open') }
  }, [open])

  return (
    <dialog ref={dialogRef} className={studentUi.quizDialog} aria-labelledby="quiz-submit-title" onCancel={(event) => { event.preventDefault(); if (!submitting) onCancel() }}>
      <div className="p-6">
        <span className="grid size-12 place-items-center rounded-2xl bg-student-primary/12 text-student-primary"><Send size={22} aria-hidden="true" /></span>
        <h2 id="quiz-submit-title" className="mt-4 font-student-display text-xl font-bold text-student-ink">Nộp bài?</h2>
        <p className={`mt-2 ${studentUi.body}`}>Bạn đã trả lời <strong className="text-student-ink">{answered}/{total}</strong> câu. Sau khi nộp, bài làm được chấm ngay và không thể sửa.</p>
        {blankIndexes.length ? (
          <div className="mt-4 rounded-2xl bg-student-bookmark/15 p-4">
            <p className="flex items-center gap-2 text-sm font-bold text-student-ink"><AlertTriangle size={16} aria-hidden="true" />Còn {blankIndexes.length} câu chưa trả lời</p>
            <div className="mt-3 flex flex-wrap gap-2">
              {blankIndexes.map((index) => (
                <button key={index} type="button" onClick={() => onGo(index)} className="min-h-9 cursor-pointer rounded-xl border border-student-line bg-student-surface px-3 text-sm font-bold text-student-primary-strong outline-none hover:bg-student-surface-muted focus-visible:ring-4 focus-visible:ring-student-primary/20">Câu {index + 1}</button>
              ))}
            </div>
          </div>
        ) : null}
        {error ? <p role="alert" className={`mt-4 ${studentUi.error}`}>{error.message}</p> : null}
        <div className="mt-6 flex flex-wrap justify-end gap-3">
          <button type="button" disabled={submitting} onClick={onCancel} className={studentUi.quizGhostButton}>Tiếp tục làm</button>
          <button type="button" disabled={submitting} onClick={onConfirm} className={studentUi.quizPrimaryButton}>{submitting ? 'Đang nộp…' : 'Nộp bài'}</button>
        </div>
      </div>
    </dialog>
  )
}
