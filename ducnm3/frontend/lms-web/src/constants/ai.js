import { PAGINATION } from '@/constants/pagination'

export const AI_STATUS = {
  active: 'ACTIVE',
  inactive: 'INACTIVE',
}

export const AI_STATUS_LABELS = {
  [AI_STATUS.active]: 'Đang bật',
  [AI_STATUS.inactive]: 'Đã tắt',
}

export const AI_HEALTH_STATUS = {
  online: 'ONLINE',
  authFailed: 'AUTH_FAILED',
  unreachable: 'UNREACHABLE',
  error: 'ERROR',
  disabled: 'DISABLED',
}

export const AI_HEALTH_LABELS = {
  [AI_HEALTH_STATUS.online]: 'Hoạt động',
  [AI_HEALTH_STATUS.authFailed]: 'Key bị từ chối',
  [AI_HEALTH_STATUS.unreachable]: 'Không kết nối được',
  [AI_HEALTH_STATUS.error]: 'Lỗi',
  [AI_HEALTH_STATUS.disabled]: 'Đã tắt',
}

export const AI_HEALTH_HINTS = {
  [AI_HEALTH_STATUS.online]: 'Provider phản hồi bình thường.',
  [AI_HEALTH_STATUS.authFailed]: 'Provider từ chối API key đang lưu. Nhập lại key.',
  [AI_HEALTH_STATUS.unreachable]: 'Không kết nối được tới base URL hoặc quá thời gian chờ.',
  [AI_HEALTH_STATUS.error]: 'Provider trả lỗi hoặc cấu hình không dùng được.',
  [AI_HEALTH_STATUS.disabled]: 'Provider đang tắt nên không được thử kết nối.',
}

export const AI_PROVIDER_TYPE = {
  openai: 'OPENAI',
  gemini: 'GEMINI',
  anthropic: 'ANTHROPIC',
  mistral: 'MISTRAL',
  custom: 'CUSTOM',
}

export const AI_PROVIDER_TYPE_LABELS = {
  [AI_PROVIDER_TYPE.openai]: 'OpenAI (GPT)',
  [AI_PROVIDER_TYPE.gemini]: 'Google Gemini',
  [AI_PROVIDER_TYPE.anthropic]: 'Anthropic Claude',
  [AI_PROVIDER_TYPE.mistral]: 'Mistral AI',
  [AI_PROVIDER_TYPE.custom]: 'Tùy chỉnh (OpenAI-compatible)',
}

export const AI_LIST_DEFAULT_QUERY = {
  page: PAGINATION.defaults.page,
  pageSize: PAGINATION.defaults.pageSize,
}

// Lấy đủ để đổ vào dropdown chọn provider/model; trần pageSize của API là 100.
export const AI_CHOICES_QUERY = {
  page: PAGINATION.defaults.page,
  pageSize: PAGINATION.limits.pageSizeMax,
}

export const AI_COPY = {
  providerItem: 'provider',
  modelItem: 'model',
  name: 'Tên provider',
  providerType: 'Loại AI',
  baseUrl: 'Base URL',
  baseUrlHint: 'Bỏ trống để dùng endpoint mặc định của loại AI. Loại Tùy chỉnh bắt buộc nhập.',
  apiKey: 'API key',
  apiKeyCreateHint: 'Chỉ gửi một lần, server mã hóa rồi lưu; không thể xem lại.',
  apiKeyEditHint: 'Bỏ trống để giữ key đang lưu.',
  status: 'Trạng thái',
  provider: 'Provider',
  modelName: 'Tên model',
  modelNameHint: 'Đúng tên model của provider, ví dụ gpt-4o hoặc gemini-2.0-flash.',
  displayName: 'Tên hiển thị',
  displayNameHint: 'Bỏ trống để dùng tên model.',
  service: 'Service',
  model: 'Model',
  search: 'Tìm kiếm',
  createProvider: 'Tạo provider',
  createModel: 'Tạo model',
  saveChanges: 'Lưu thay đổi',
  cancelEdit: 'Hủy sửa',
  assign: 'Gán model',
  edit: 'Sửa',
  connection: 'Kết nối',
  recheck: 'Kiểm tra lại',
  recheckAll: 'Kiểm tra kết nối',
  checking: 'Đang thử…',
  remove: 'Xóa',
}

export const AI_PAGES = {
  eyebrow: 'AI',
  providers: {
    title: 'AI provider',
    description: 'Khai báo loại AI và API key. Key được mã hóa ở server, chỉ hiện 4 ký tự cuối.',
    searchHint: 'Tìm theo tên provider.',
    searchPlaceholder: 'Ví dụ: OpenAI production',
    emptyTitle: 'Chưa có provider',
    emptyDescription: 'Tạo provider đầu tiên ở bảng Input.',
  },
  models: {
    title: 'AI model',
    description: 'Mỗi model thuộc một provider; service chỉ được gán model đang bật.',
    searchHint: 'Tìm theo tên model hoặc tên hiển thị.',
    searchPlaceholder: 'Ví dụ: gpt-4o',
    emptyTitle: 'Chưa có model',
    emptyDescription: 'Tạo provider trước, rồi thêm model ở bảng Input.',
  },
  bindings: {
    title: 'Gán AI cho service',
    description: 'Chọn service rồi chọn model để service đó dùng AI. Mỗi service một model.',
    emptyTitle: 'Chưa gán service nào',
    emptyDescription: 'Chọn service và model ở bảng Input rồi bấm Gán model.',
    noModels: 'Chưa có model đang bật. Hãy tạo provider và model trước.',
  },
  deleteProvider: {
    title: 'Xóa provider?',
    description: 'Provider còn model sẽ không xóa được. Thao tác này không hoàn tác.',
  },
  deleteModel: {
    title: 'Xóa model?',
    description: 'Model đang được gán cho service sẽ không xóa được. Thao tác này không hoàn tác.',
  },
  deleteBinding: {
    title: 'Gỡ AI khỏi service?',
    description: 'Service sẽ không còn model AI nào được gán.',
  },
}
