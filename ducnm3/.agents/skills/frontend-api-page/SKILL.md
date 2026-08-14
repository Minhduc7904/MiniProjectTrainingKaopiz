---
name: frontend-api-page
description: Tạo trang frontend quản trị cho đúng một API (một menu sidebar = một endpoint). Workbench Input/Output, Redux, toast httpClient, UML Activity Diagram, Lucide, theme `ui`. Dùng khi thêm page lms-web, clone trang Sổ học viên, gắn service menu, hoặc làm form gọi API.
---

# Frontend API page

## Khi sử dụng

Một page mới trên `frontend/lms-web` tương ứng **đúng một API**. Mẫu chuẩn là
trang `GET /student/api/students`
(`pages/students/StudentsPage.jsx`).

Không dùng skill này để sửa BE endpoint; BE dùng `api-*-endpoint`. Không dùng
thay `interface-design` — phải đọc cả hai.

## Tài liệu bắt buộc

Đọc hết rồi mới code:

1. [`reference.md`](reference.md) — map file, rule, theme, UML.
2. [`template.md`](template.md) — checklist và skeleton.
3. `rules/frontend.md`, `rules/documentation-language.md`.
4. `.agents/skills/interface-design/` (cả folder, bắt đầu `SKILL.md`).
5. `docs/architecture/frontend.md`, `.interface-design/system.md`.
6. API doc + business-flow **của đúng endpoint** (1:1).
7. Skill HTTP method tương ứng nếu contract BE đổi.
8. Từ Ngày 3: `.agents/skills/developer-task/`.

Mẫu sống: copy cấu trúc `students/`, không bịa layout khác.

## Quy trình

### 1. Chốt một API

- Một `SERVICES[].menus[]` item = một route = một page = một API.
- Ghi path Gateway, method, query/body, pagination, error code.
- Không gộp hai endpoint trên một page.

### 2. Constants trước code UI

- `API_ROUTES`, `APP_ROUTES`, `QUERY_PARAMS`, enum allowlist.
- Input schema: `constants/inputs/<api>.js` (nullable, default, allowlist).
- Default query object dùng cho slice **và** nút Reset.
- Activity Diagram: `constants/activities/<api>.js` khớp business-flow UML.
- Copy/label: `constants/<domain>Copy.js`, `UI_LABELS` nếu dùng chung.
- Không hard-code path, query name, status, hex.

### 3. API + Redux + hook

- `api/<domain>Api.js` chỉ `httpClient` + `unwrapEnvelope`. Page không Axios.
- List/detail: `data`, `pagination`, `query`, `loading`, `success`, `error`,
  `traceId` trong Redux (`createApiListSlice` khi là list).
- Hook `hooks/<domain>/useX.js`: `setQuery`, `load`/`submit`, `reset`.
- `query` là draft Input dùng chung: tab Mẫu và Thủ công đọc/ghi cùng object.
  Không `useState` draft riêng từng tab. File multipart giữ ở page state, chia
  sẻ giữa hai tab (không đưa `File` vào Redux).
- Toast: interceptor `httpClient`, không toast trong page.
- Gắn reducer vào `app/store.js`.

### 4. Shell và menu

- `SERVICES` + `activityId` trên menu.
- Route trong `app/router.jsx` (không dùng `PlaceholderPage` khi API sẵn sàng).
- Icon Lucide map trong `Sidebar.jsx`.

### 5. Page Workbench

Bọc `Workbench` → `InputPanel` + `OutputPanel`.

Input:

- Tab **Mẫu**: control sẵn (Dropdown/Pagination) + `PageHeader`. Đổi field thì
  `setQuery` (GET list thường `load` luôn).
- Tab **Thủ công**: `ApiField` controlled từ `query`; `onChange={setQuery}`.
- Hai tab luôn cùng giá trị. `InputPanel` ẩn tab không chọn (`hidden`), không
  unmount, để không mất state native (file input).
- Nút **Reset**: GET = default query + gọi API lại; POST = default + xóa kết
  quả, không tự gọi.
- Tab thủ công: nút **Gọi API** `load(query)` / `submit` từ draft chung.

Output:

- Tab **JSON**: envelope từ Redux (`JsonView`).
- Tab **Xem**: bảng/empty/skeleton thân thiện.
- Tab **UML**: `ActivityDiagram` + `run={{ loading, success, error }}`.

Màu chỉ qua `import { ui } from '@/theme'`. Icon chỉ Lucide qua `Icon`.
Mọi chỗ ấn được: `cursor-pointer`.

### 6. UML từ BE docs

Vẽ Activity Diagram theo `docs/business-flows/...` và error observability:

- Swimlane đúng participant (Client, Gateway, service, Repo, DB, Fault).
- Success: tô `successPath`.
- Error: `failByCode[error.code]` trỏ đúng nhánh (400, 503, timeout, network).
- List rỗng = 200, không nhánh 404 trừ khi API doc nói vậy.

### 7. Docs

Cập nhật `docs/architecture/frontend.md` nếu thêm pattern/file dùng chung.

## Definition of done

- [ ] Một menu = một API = một page.
- [ ] Constants, api, slice, hook, page, page components đủ.
- [ ] Input Mẫu + Thủ công cùng `query`; Reset (GET refetch; POST không tự gọi).
- [ ] Output JSON + Xem + UML theo docs BE.
- [ ] Không Axios trong page; list state trong Redux; toast global.
- [ ] Theme `ui` + Lucide; `npm run lint` và `npm run build` pass.
