import { useEffect, useRef } from 'react'
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

  useEffect(() => {
    const mathElements = contentRef.current?.querySelectorAll('.course-math[data-math-display][data-math-tex]') ?? []

    mathElements.forEach((element) => {
      katex.render(element.dataset.mathTex, element, {
        ...mathRenderOptions,
        displayMode: element.dataset.mathDisplay === 'block',
      })
    })
  }, [html])

  if (!html) return <p className={`text-[14px] ${adminUi.body}`}>{emptyLabel}</p>
  return <div ref={contentRef} className={className} dangerouslySetInnerHTML={{ __html: html }} />
}
