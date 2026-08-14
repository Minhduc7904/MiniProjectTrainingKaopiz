# Frontend API page — reference

Mẫu sống: `frontend/lms-web/src/pages/students/` cho
`GET /student/api/students`.

## Rule bắt buộc (`rules/frontend.md`)

- Code chỉ trong `frontend/lms-web/`.
- Page không gọi Axios; chỉ `src/api/` + hook.
- `data`, `pagination`, `query`, `loading`, `success`, `error` của list/detail
  ở Redux, không local. `query` là draft Input chung cho tab Mẫu và Thủ công.
- Toast chỉ qua `httpClient` interceptor.
- Không hard-code path/header/status/query name → `src/constants/`.
- Env chỉ `VITE_*`.
- Hex chỉ `src/theme/tokens.css`. Class màu chỉ `ui` từ `@/theme`.
- Clickable: `cursor-pointer`; disabled: `cursor-not-allowed`.
- Icon: Lucide qua `components/ui/Icon.jsx`.

## Layout quản trị

```text
AppShell
  Sidebar fixed (header + Service dropdown + menu cuộn + footer cố định)
  main
    Workbench
      InputPanel   tab Mẫu | Thủ công, nút Reset
      OutputPanel  tab JSON | Xem | UML
```

Một `SERVICES[].menus[]` = một `APP_ROUTES` = một page = một API.

## Map file mẫu (students)

| Vai trò | File |
| --- | --- |
| Gateway path | `constants/apiRoutes.js` |
| UI route + menu | `constants/appRoutes.js` (`activityId`) |
| Query/enum | `constants/queryParams.js`, `studentStatus.js` |
| Input schema + default query | `constants/inputs/getStudents.js` |
| UML graph | `constants/activities/getStudents.js` |
| Copy | `constants/studentCopy.js` |
| HTTP | `api/studentsApi.js` + `unwrapEnvelope` |
| Slice | `features/students/studentsSlice.js` + `createApiListSlice.js` |
| Hook | `hooks/students/useStudentsList.js` (`setQuery`, `load`, `reset`) |
| Page | `pages/students/StudentsPage.jsx` |
| Mẫu filters | `pages/students/components/StudentsFilters.jsx` |
| Thủ công | `pages/students/components/StudentsManualForm.jsx` |
| Xem | `pages/students/components/StudentsTable.jsx` |
| Store | `app/store.js` |
| Router | `app/router.jsx` |

UI dùng chung (không copy): `Workbench`, `InputPanel`, `OutputPanel`,
`ApiField`, `Dropdown`, `Pagination`, `JsonView`, `ActivityDiagram`, `Button`,
`Field`, `Tabs`, `Icon`, `EmptyState`, `TableSkeleton`.

## Input schema

Mỗi field:

```text
key          QUERY_PARAMS / body field
label        copy tiếng Việt
type         string | integer | ...
required     boolean
nullable     boolean (rỗng = không gửi / không lọc)
defaultValue null hoặc default server
allowlist    array hoặc null
hint         rule từ API doc
```

GET list: không có body. POST: field body, ghi required/nullable theo contract.

## Input tabs (bắt buộc đồng bộ)

`query` trong Redux là nguồn duy nhất cho tab **Mẫu** và **Thủ công**.

- Cả hai tab controlled: `value={query[field]}`, `onChange` → `setQuery`.
- Không `useState` draft local trên từng form.
- GET: đổi Dropdown/Pagination ở Mẫu có thể `load` ngay (pending ghi `query`).
  Thủ công `setQuery` từng phím; **Gọi API** mới `load(query)`.
- POST: `setQuery` khi sửa field; `File` ở state trang, cùng `file` cho hai tab.
  Hiện tên file đã chọn trên cả hai `FileInput`.
- `InputPanel` render cả hai tab, tab không chọn dùng `hidden` — không unmount.
- Reset ghi default `query` (và `fileKey++` nếu có file).

## Reset

GET list: `reset` dispatch default query rồi gọi API lại.

POST/multipart: `reset` khôi phục default field, xóa `data`/`error`, remount
file input (`fileKey`). **Không** tự POST — request không idempotent.

Default query **một chỗ** (constants), slice `initialState` và nút Reset dùng
chung. Form thủ công controlled từ `query`, không sync `useEffect` draft local.

## UML

`failByCode` map `API_ERROR_CODES` → node Fault:

| Code | Nhánh điển hình |
| --- | --- |
| `NETWORK_ERROR` | Client → Gateway |
| `REQUEST_TIMEOUT` | Gateway timeout |
| `SERVICE_UNAVAILABLE` | Gateway hạ nguồn |
| `VALIDATION_FAILED` | 400 query/body |
| `DATABASE_UNAVAILABLE` | DB |
| `UNEXPECTED_ERROR` | 500 |

Success path tô accent; nhánh không đi skipped; fail tô danger. List rỗng:
200 + `data []`, không 404 trừ khi API doc nói.

Nguồn: `docs/business-flows/<domain>/` (UML sequence) +
`docs/api/shared/error-handling-observability.md`.

## Theme

Import `{ ui } from '@/theme'`. Không `bg-gray-*`, không hex trong JSX.
Token mới chỉ thêm `theme/tokens.css` rồi map `theme/ui.js`.
