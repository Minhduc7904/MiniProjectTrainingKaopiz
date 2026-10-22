import { useEffect, useMemo, useRef } from 'react'
import katex from 'katex'
import 'katex/dist/katex.min.css'
import { adminUi } from '@/theme/admin'

const mathRenderOptions = Object.freeze({
  throwOnError: false,
  trust: false,
  strict: 'ignore',
})

export function RenderedMarkdown({ html, emptyLabel = 'Chưa có nội dung.', className = adminUi.markdown }) {
  const contentRef = useRef(null)
  // Giữ nguyên object để React không gán lại innerHTML (xóa công thức KaTeX đã vẽ) mỗi lần cha render lại.
  const markup = useMemo(() => ({ __html: html ?? '' }), [html])

  // Chạy sau mỗi lần commit (không phụ thuộc `html`): React có thể dựng lại DOM mà `html` không đổi,
  // khi đó span công thức trống và effect theo `[html]` sẽ không chạy lại. Span đã render được bỏ qua.
  useEffect(() => {
    const mathElements = contentRef.current?.querySelectorAll('.course-math[data-math-display][data-math-tex]:not([data-math-rendered])') ?? []

    mathElements.forEach((element) => {
      katex.render(element.dataset.mathTex, element, {
        ...mathRenderOptions,
        displayMode: element.dataset.mathDisplay === 'block',
      })
      element.dataset.mathRendered = 'true'
    })
  })

  if (!html) return <p className={`text-[14px] ${adminUi.body}`}>{emptyLabel}</p>
  return <div ref={contentRef} className={className} dangerouslySetInnerHTML={markup} />
}
