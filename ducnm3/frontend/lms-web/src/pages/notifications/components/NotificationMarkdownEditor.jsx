import { useMemo, useState } from 'react'
import { ImagePlus } from 'lucide-react'
import { Button } from '@/components/ui/Button'
import { FieldLabel, FileInput, TextInput } from '@/components/ui/Field'
import { Icon } from '@/components/ui/Icon'
import { ui } from '@/theme'

const EMBED_IMAGE_PATTERN = /!\[([^\]]*)\]\(([^\s)]+)\)/g

function embeddedImages(markdown) {
  return Array.from(markdown.matchAll(EMBED_IMAGE_PATTERN)).map((match, index) => ({
    index,
    alt: match[1],
    url: match[2],
  }))
}

function replaceAlt(markdown, targetIndex, nextAlt) {
  let currentIndex = 0
  const safeAlt = nextAlt.replaceAll(']', '')
  return markdown.replace(EMBED_IMAGE_PATTERN, (fullMatch, alt, url) => {
    const replacement = currentIndex === targetIndex ? `![${safeAlt}](${url})` : fullMatch
    currentIndex += 1
    return replacement
  })
}

export function NotificationMarkdownEditor({
  value,
  disabled,
  uploadLoading,
  uploadError,
  onChange,
  onUploadImage,
}) {
  const [file, setFile] = useState(null)
  const [alt, setAlt] = useState('')
  const images = useMemo(() => embeddedImages(value), [value])

  const upload = async () => {
    if (!file) return
    const media = await onUploadImage(file)
    if (!media?.contentUrl) return

    const label = alt.trim() || file.name
    const gap = value && !value.endsWith('\n') ? '\n\n' : ''
    onChange(`${value}${gap}![${label}](${media.contentUrl})`)
    setFile(null)
    setAlt('')
  }

  return (
    <section className={`rounded-lg ${ui.card} p-4`}>
      <div className="flex flex-wrap items-start justify-between gap-3">
        <div>
          <p className={`font-display text-[16px] font-semibold ${ui.title}`}>
            Soạn Markdown & ảnh nhúng
          </p>
          <p className={`mt-1 text-[12px] ${ui.body}`}>
            Ảnh được chèn dưới dạng <code>![alt](contentUrl)</code> an toàn của Media Service.
          </p>
        </div>
        <Icon icon={ImagePlus} className={ui.brand} />
      </div>

      <textarea
        id="batch-body"
        name="bodyMarkdown"
        value={value}
        disabled={disabled}
        placeholder="Nội dung gửi cho học viên..."
        onChange={(event) => onChange(event.target.value)}
        className={`${ui.control} mt-4 h-40 resize-y py-2`}
      />

      <div className={`mt-4 grid gap-3 border-t pt-4 ${ui.hairlineT}`}>
        <div className="flex flex-col gap-1">
          <FieldLabel htmlFor="markdown-image" hint="Chỉ nhận ảnh; Media API kiểm tra MIME và kích thước.">
            Ảnh nhúng
          </FieldLabel>
          <FileInput
            id="markdown-image"
            name="markdownImage"
            accept="image/*"
            disabled={disabled || uploadLoading}
            fileName={file?.name}
            onChange={(event) => setFile(event.target.files?.[0] ?? null)}
          />
        </div>
        <div className="flex flex-col gap-1">
          <FieldLabel htmlFor="markdown-image-alt" hint="Bạn có thể sửa lại alt sau khi ảnh đã chèn.">
            Alt text
          </FieldLabel>
          <TextInput
            id="markdown-image-alt"
            name="imageAlt"
            value={alt}
            disabled={disabled || uploadLoading}
            placeholder="Mô tả ảnh cho người đọc"
            onChange={(event) => setAlt(event.target.value)}
          />
        </div>
        <div>
          <Button disabled={disabled || uploadLoading || !file} onClick={upload}>
            <Icon icon={ImagePlus} />
            {uploadLoading ? 'Đang tải ảnh' : 'Tải ảnh và chèn Markdown'}
          </Button>
        </div>
        {uploadError ? (
          <p className={`rounded-md ${ui.badgeDanger} px-3 py-2 text-[13px]`}>
            {uploadError.code}: {uploadError.message}
          </p>
        ) : null}
      </div>

      {images.length ? (
        <div className={`mt-4 border-t pt-4 ${ui.hairlineT}`}>
          <p className={`text-[12px] font-medium ${ui.title}`}>Ảnh đã chèn</p>
          <div className="mt-2 flex flex-col gap-2">
            {images.map((image) => (
              <div key={`${image.url}-${image.index}`} className={`rounded-md ${ui.card} p-3`}>
                <p className={`truncate font-mono text-[11px] ${ui.caption}`}>{image.url}</p>
                <div className="mt-2 flex flex-col gap-1">
                  <FieldLabel htmlFor={`embedded-alt-${image.index}`}>Alt text</FieldLabel>
                  <TextInput
                    id={`embedded-alt-${image.index}`}
                    name={`embeddedAlt${image.index}`}
                    value={image.alt}
                    disabled={disabled}
                    onChange={(event) => onChange(replaceAlt(value, image.index, event.target.value))}
                  />
                </div>
              </div>
            ))}
          </div>
        </div>
      ) : null}
    </section>
  )
}
