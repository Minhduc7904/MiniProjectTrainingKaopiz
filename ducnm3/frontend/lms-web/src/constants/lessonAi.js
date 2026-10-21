export const LESSON_AI_DOCUMENT_STATUS = {
  notQueued: 'NOT_QUEUED',
  pending: 'PENDING',
  extracting: 'EXTRACTING',
  extracted: 'EXTRACTED',
  failed: 'FAILED',
  unsupported: 'UNSUPPORTED',
}

export const LESSON_AI_DOCUMENT_STATUS_LABELS = {
  [LESSON_AI_DOCUMENT_STATUS.notQueued]: 'Chưa xếp hàng',
  [LESSON_AI_DOCUMENT_STATUS.pending]: 'Đang chờ',
  [LESSON_AI_DOCUMENT_STATUS.extracting]: 'Đang trích xuất',
  [LESSON_AI_DOCUMENT_STATUS.extracted]: 'Đã trích xuất',
  [LESSON_AI_DOCUMENT_STATUS.failed]: 'Lỗi',
  [LESSON_AI_DOCUMENT_STATUS.unsupported]: 'Không hỗ trợ',
}

export const LESSON_AI_JOB_TYPE = {
  synthesize: 'SYNTHESIZE_LESSON',
  quiz: 'GENERATE_QUIZ',
}

export const LESSON_AI_JOB_TYPE_LABELS = {
  [LESSON_AI_JOB_TYPE.synthesize]: 'Tổng hợp bài học',
  [LESSON_AI_JOB_TYPE.quiz]: 'Tạo quiz',
}

export const LESSON_AI_JOB_STATUS = {
  pending: 'PENDING',
  running: 'RUNNING',
  completed: 'COMPLETED',
  failed: 'FAILED',
}

export const LESSON_AI_JOB_STATUS_LABELS = {
  [LESSON_AI_JOB_STATUS.pending]: 'Đang chờ',
  [LESSON_AI_JOB_STATUS.running]: 'Đang chạy',
  [LESSON_AI_JOB_STATUS.completed]: 'Hoàn tất',
  [LESSON_AI_JOB_STATUS.failed]: 'Lỗi',
}

export const LESSON_AI_ERROR_CODES = {
  contentExists: 'LESSON_CONTENT_EXISTS',
}

// Mã lỗi hay gặp khi AI Service/provider chưa sẵn sàng; mã khác hiện thông điệp gốc từ server.
export const LESSON_AI_ERROR_HINTS = {
  AI_TASK_NOT_CONFIGURED: 'Chưa gán model cho tác vụ này. Vào AI → Gán model để cấu hình.',
  AI_REFERENCE_INACTIVE: 'Provider hoặc model đang bị tắt. Bật lại trong trang AI.',
  AI_PROVIDER_AUTH_FAILED: 'Provider từ chối API key. Nhập lại key trong trang AI.',
  AI_PROVIDER_RATE_LIMITED: 'Provider đang giới hạn tốc độ. Thử lại sau ít phút.',
  AI_PROVIDER_TIMEOUT: 'Provider phản hồi quá lâu. Thử lại hoặc chọn tài liệu ngắn hơn.',
  AI_PROVIDER_UNREACHABLE: 'Không kết nối được tới provider.',
  AI_PROVIDER_REJECTED_REQUEST: 'Model không hỗ trợ loại tài liệu hoặc tool này.',
  MEDIA_TOO_LARGE: 'Tệp lớn hơn giới hạn 15 MB cho AI.',
  MEDIA_NOT_FOUND: 'Tệp không còn tồn tại.',
  DOCUMENTS_TOO_LARGE: 'Các tài liệu đã chọn quá dài. Chọn ít tài liệu hơn.',
  EMPTY_DOCUMENT: 'Không đọc được chữ nào từ tài liệu này.',
  UNSUPPORTED_FILE_TYPE: 'Chỉ hỗ trợ PDF, DOCX, TXT, MD và ảnh PNG/JPG/WEBP/GIF.',
  WORKER_INTERRUPTED: 'Tiến trình xử lý bị gián đoạn. Thử lại.',
  AI_NO_RESULT: 'Model không lưu kết quả. Thử lại.',
  AI_TOO_MANY_STEPS: 'Model không tạo được kết quả hợp lệ sau nhiều lần thử. Thử lại.',
}

export const LESSON_AI_POLL_INTERVAL_MS = 3000

export const LESSON_AI_QUESTION_COUNT = { min: 1, max: 30, default: 10 }

export const LESSON_AI_COPY = {
  title: 'Soạn nội dung bằng AI',
  hint: 'AI đọc các tài liệu đính kèm bạn chọn. Học viên vẫn xem được tài liệu gốc.',
  reviewNotice: 'Nội dung do AI tạo — cần kiểm tra trước khi cho học viên dùng.',
  documentsHeading: 'Chọn tài liệu để AI sử dụng',
  noAttachments: 'Gắn tài liệu vào lesson để dùng AI.',
  selectAll: 'Chọn tất cả đã trích xuất',
  clearSelection: 'Bỏ chọn',
  retryFailed: 'Thử lại tài liệu lỗi',
  selectedCount: (count) => `Đã chọn ${count} tài liệu`,
  needSelection: 'Chọn ít nhất một tài liệu đã trích xuất.',
  synthesize: 'Tự động tổng hợp bài học',
  generateQuiz: 'Tự động tạo quiz',
  questionCount: 'Số câu hỏi',
  openQuiz: 'Mở quiz nháp',
  overwriteTitle: 'Ghi đè nội dung lesson?',
  overwriteDescription: 'Lesson đã có nội dung. AI sẽ thay thế toàn bộ; bản cũ được lưu trong lịch sử job để khôi phục.',
  overwriteConfirm: 'Ghi đè',
  jobsHeading: 'Lần chạy gần đây',
  noJobs: 'Chưa có lần chạy nào.',
  loadFailed: 'Không tải được trạng thái tài liệu AI.',
  restoreHint: 'Nội dung cũ được lưu theo job này.',
}
