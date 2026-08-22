import { useEffect, useMemo, useState } from 'react'
import { Grid2X2, List, LoaderCircle, RefreshCw } from 'lucide-react'
import { Button } from '@/components/ui/admin/Button'
import { EmptyState } from '@/components/ui/admin/EmptyState'
import { Icon } from '@/components/ui/admin/Icon'
import { PageHeader } from '@/components/ui/admin/PageHeader'
import { useMediaLibraryExplorer } from '@/hooks/media/useMediaLibraryExplorer'
import { MediaLibraryDetailPanel } from './components/MediaLibraryDetailPanel'
import { MediaLibraryExplorerFilters } from './components/MediaLibraryExplorerFilters'
import { MediaLibraryExplorerList } from './components/MediaLibraryExplorerList'
import { adminUi } from '@/theme/admin'

export function MediaLibraryPage() {
  const library = useMediaLibraryExplorer()
  const [viewMode, setViewMode] = useState('grid')
  const { data, selectedId, select } = library
  const selectedMedia = useMemo(
    () => data.find((media) => media.id === selectedId) ?? null,
    [data, selectedId],
  )

  useEffect(() => {
    if (!selectedId && data.length) {
      select(data[0].id)
    }
  }, [data, selectedId, select])

  return (
    <div className={`h-full overflow-hidden p-5 ${adminUi.page}`}>
      <div className="flex h-full min-h-0 flex-col">
        <div className="flex shrink-0 items-start justify-between gap-4">
          <PageHeader
            eyebrow="Media"
            title="Thư viện Media"
            description="Duyệt media đã lưu, lọc theo loại và trạng thái, sau đó chọn một tệp để xem chi tiết."
          />
          <Button variant="ghost" disabled={library.loading} onClick={library.reload} aria-label="Làm mới thư viện media">
            <Icon icon={library.loading ? LoaderCircle : RefreshCw} className={library.loading ? adminUi.spinner : undefined} />Làm mới
          </Button>
        </div>

        <section className={`mt-5 grid min-h-0 flex-1 overflow-hidden rounded-lg ${adminUi.card} xl:grid-cols-[minmax(340px,0.9fr)_minmax(0,1.5fr)]`}>
          <div className={`flex min-h-0 flex-col ${adminUi.panelSplit}`}>
            <MediaLibraryExplorerFilters query={library.query} disabled={library.loading} onChange={library.setQuery} />
            <div className={`flex items-center justify-between gap-3 px-4 py-3 ${adminUi.hairlineT} ${adminUi.hairlineB}`}>
              <p className={`text-[12px] ${adminUi.body}`}>{library.data.length} media phù hợp</p>
              <div className="flex rounded-md border border-line p-0.5" aria-label="Kiểu hiển thị">
                <button type="button" aria-label="Hiển thị dạng lưới" aria-pressed={viewMode === 'grid'} onClick={() => setViewMode('grid')} className={`flex h-8 w-8 cursor-pointer items-center justify-center rounded ${viewMode === 'grid' ? adminUi.navActive : adminUi.navIdle}`}><Icon icon={Grid2X2} size={16} /></button>
                <button type="button" aria-label="Hiển thị dạng danh sách" aria-pressed={viewMode === 'list'} onClick={() => setViewMode('list')} className={`flex h-8 w-8 cursor-pointer items-center justify-center rounded ${viewMode === 'list' ? adminUi.navActive : adminUi.navIdle}`}><Icon icon={List} size={16} /></button>
              </div>
            </div>

            <div className="min-h-0 flex-1 overflow-y-auto py-4">
              {library.error ? <div className="px-4"><EmptyState title="Không đọc được thư viện media" description={`${library.error.code}: ${library.error.message}`} /></div> : null}
              {!library.error && library.loading && !library.data.length ? <div className={`flex items-center justify-center gap-2 px-4 py-10 text-[13px] ${adminUi.body}`}><Icon icon={LoaderCircle} className={adminUi.spinner} />Đang tải media…</div> : null}
              {!library.error && !library.loading && library.success && !library.data.length ? <div className="px-4"><EmptyState title="Chưa có media phù hợp" description="Thử thay đổi bộ lọc loại media hoặc trạng thái." /></div> : null}
              {!library.error && library.data.length ? <MediaLibraryExplorerList items={library.data} selectedId={library.selectedId} viewMode={viewMode} onSelect={library.select} /> : null}
            </div>

            {library.nextCursor ? <div className={`shrink-0 px-4 py-3 ${adminUi.hairlineT}`}><Button variant="ghost" disabled={library.loading} onClick={library.loadMore}>Xem thêm</Button></div> : null}
          </div>
          <MediaLibraryDetailPanel media={selectedMedia} />
        </section>
      </div>
    </div>
  )
}
