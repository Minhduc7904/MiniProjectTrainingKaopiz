import { describe, expect, it } from 'vitest'
import {
  APP_ROUTES,
  getMenuByPath,
  notificationBatchProgressPath,
  SERVICES,
} from './appRoutes'

describe('notification menu routing', () => {
  it('exposes three independent notification menus', () => {
    const notification = SERVICES.find((service) => service.id === 'notification')
    expect(notification.menus.map((menu) => menu.label)).toEqual([
      'Gửi hàng loạt',
      'Quản lý batch',
      'Tiến trình gửi',
    ])
  })

  it.each([
    [APP_ROUTES.notificationSend, 'Gửi hàng loạt'],
    [APP_ROUTES.notificationBatches, 'Quản lý batch'],
    [APP_ROUTES.notificationProgress, 'Tiến trình gửi'],
    [notificationBatchProgressPath('batch-123'), 'Tiến trình gửi'],
  ])('selects only the owning menu for %s', (pathname, label) => {
    expect(getMenuByPath(pathname).label).toBe(label)
  })
})

describe('course menu routing', () => {
  it('selects the CSV export menu instead of the dynamic course-detail menu', () => {
    expect(getMenuByPath(APP_ROUTES.courseExport).label).toBe('Xuất CSV khóa học')
  })
})

describe('media menu routing', () => {
  it('exposes the media library menu', () => {
    expect(getMenuByPath(APP_ROUTES.mediaLibrary).label).toBe('Thư viện Media')
  })

  it('exposes the media job management menu', () => {
    expect(getMenuByPath(APP_ROUTES.mediaJobs).label).toBe('Quản lý job')
  })
})
