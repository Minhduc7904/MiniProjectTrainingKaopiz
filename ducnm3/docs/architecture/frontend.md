# Kiến trúc frontend (`lms-web`)

Frontend Mini LMS là một SPA React, gọi API qua YARP Gateway. App không nói
chuyện trực tiếp với từng microservice.

## Ngăn xếp

```text
Vite + React (JavaScript)
Tailwind CSS
Lucide (`lucide-react`)
Axios
Redux Toolkit
React Router
```

Chạy local:

```bash
cd frontend/lms-web
cp .env.example .env
npm install
npm run dev
```

`VITE_API_BASE_URL` mặc định là `http://localhost:5100` (API Gateway).

## Cấu trúc folder

```text
frontend/lms-web/
├── .env.example
├── vite.config.js
└── src/
    ├── app/                 # store, router, providers
    ├── api/                 # Axios client và hàm gọi endpoint
    ├── constants/           # Mọi string/path/status dùng lại
    ├── features/            # Redux slice theo domain
    ├── hooks/               # Hook gọi API, đọc Redux
    ├── theme/               # tokens.css (hex) + ui.js (class semantic)
    ├── components/
    │   ├── layout/          # AppShell, Sidebar, Workbench, OutputPanel
    │   └── ui/              # Button, Field, Dropdown, Tabs, JsonView, Pagination, ...
    └── pages/
        ├── students/
        ├── media/
        └── placeholder/
```

Alias `@/` trỏ tới `src/`.

## Luồng dữ liệu API

```text
Page
  → hook (useStudentsList | useMediaUpload)
    → dispatch thunk
      → api/* (Axios)
        → Gateway
    ← Redux: data, pagination?, loading, success, error
  → page components (chỉ nhận props)
```

Quy tắc:

- Page không gọi Axios.
- Component UI không biết Redux.
- Hook là chỗ duy nhất page dùng để load API.
- `data`, `pagination`, `loading`, `success`, `error` sống trong Redux slice.
- `query` là draft Input: tab Mẫu và Thủ công đọc/ghi cùng object (`setQuery`).
  Không draft `useState` riêng từng tab. `File` multipart giữ ở page, chia sẻ.

Mẫu list state:

```text
list: {
  data,
  pagination,
  query,
  loading,
  success,
  error,
  traceId
}
```

Slice list mới tái sử dụng helper trong `src/features/createApiListSlice.js`.
Slice mutation (POST upload) dùng `src/features/createApiMutationSlice.js`.
Request `FormData` không gắn `Content-Type` sẵn — interceptor xóa header để
browser đặt multipart boundary.

## Toast API toàn cục

Mọi request đi qua `httpClient` đều hiện toast (Axios interceptor, không gắn từng page).

Backend JSON:

- Thành công: `{ data, meta.traceId }` với HTTP `200`/`201`. Không có `error`.
- Lỗi: `{ error: { code, message, details }, meta.traceId }` với HTTP `400`/`404`/`409`/`413`/`500`/`503`.
- Gateway timeout xuống service: `5s` (`Lms.ApiGateway` HttpClient). Frontend khớp bằng `VITE_API_TIMEOUT_MS`.

Vòng đời toast:

1. Pending: message `Đang gọi API`, thanh progress đếm ngược đúng timeout Axios/BE.
2. Thành công: message `Thành công`, `status` HTTP, `code` `OK`/`CREATED`, ẩn sau 2s (progress chạy lại).
3. Lỗi hoặc hết timeout: `error.message`, `error.code`, HTTP status; ẩn sau 2s.
4. Hover: dừng đếm, scale `1.04` để đọc.

`ApiToastHost` bọc toàn app trong `AppProviders`. State toast nằm Redux `toasts`.

## CORS

Browser gọi Gateway `http://localhost:5100` từ Vite `http://localhost:5173`, nên
Gateway phải trả `Access-Control-Allow-Origin`. Header `X-Correlation-Id` làm
browser gửi preflight `OPTIONS`; nếu Gateway forward OPTIONS xuống Student
Service sẽ ra HTTP `405` và thiếu CORS header.

Gateway đọc `Cors:AllowedOrigins` và trả lời preflight tại Gateway
(`AddLmsCors` / `UseLmsCors`). Origin mặc định: `http://localhost:5173` và
`http://127.0.0.1:5173`.

## Axios interceptor

`attachHttpInterceptors` gắn một lần lên `httpClient`:

1. Gán `X-Correlation-Id`, mở toast pending.
2. Log từng request (method, URL, params, headers, body).
3. Log từng response (status, duration, headers, envelope) hoặc error.
4. Cập nhật toast success/error.

Bật/tắt log bằng `VITE_HTTP_LOG` (mặc định bật khi `npm run dev`).

## Constants

Không hard-code path, header, status, query name trong page/component.

| File | Nội dung |
| --- | --- |
| `constants/env.js` | `VITE_*` đã kiểm tra bắt buộc |
| `constants/apiRoutes.js` | Gateway path (`/student/api/students`, ...) |
| `constants/appRoutes.js` | Route UI |
| `constants/http.js` | Header và HTTP status |
| `constants/queryParams.js` | Tên query (`page`, `sortBy`, ...) |
| `constants/studentStatus.js` | `ACTIVE` / `INACTIVE` / `BLOCKED` |
| `constants/icons.js` | Size và strokeWidth Lucide |

Import từ `@/constants` hoặc file cụ thể.

## Trang và component

Mỗi trang tách ba lớp:

1. `pages/<name>/<Name>Page.jsx` — composition: Workbench Input / Output.
2. `pages/<name>/components/` — component chỉ thuộc trang đó.
3. `components/ui/` và `components/layout/` — dùng lại nhiều trang.

Layout trang quản trị:

- Sidebar `fixed`: header + chọn service + footer đứng yên; chỉ `menu` cuộn.
- Service đổi thì hiện menu của service đó.
- Output: cột Input (tab Mẫu / Thủ công) và cột Output (tab JSON / Xem / UML).

Khi một component trang được dùng lần thứ hai, chuyển sang `components/ui/`.

Component dùng chung hiện có:

| Component | File | Ghi chú |
| --- | --- | --- |
| `FieldLabel`, `TextInput`, `FileInput` | `components/ui/Field.jsx` | Label ledger + input 36px |
| `Dropdown` | `components/ui/Dropdown.jsx` | Custom list; click ngoài hoặc Escape thì đóng. Không dùng `<select>`/`<option>` |
| `Spinner`, `LoadingState` | `components/ui/Spinner.jsx`, `LoadingState.jsx` | Spinner phấn khi refetch |
| `Skeleton`, `TableSkeleton` | `components/ui/Skeleton.jsx` | Lần tải đầu, chưa có data |
| `Icon` | `components/ui/Icon.jsx` | Wrapper Lucide: size + strokeWidth cố định |
| `JsonView` | `components/ui/JsonView.jsx` | JSON kiểu code, số dòng |
| `ActivityDiagram` | `components/ui/ActivityDiagram.jsx` | UML Activity Diagram theo đúng 1 API |
| `Tabs` | `components/ui/Tabs.jsx` | Tab Output JSON / Xem / UML |
| `InputPanel` | `components/layout/InputPanel.jsx` | Tab Mẫu / Thủ công; tab ẩn không unmount |
| `ApiField` | `components/ui/ApiField.jsx` | Field API: nullability, default, allowlist |
| `OutputPanel` | `components/layout/OutputPanel.jsx` | Tab JSON / Xem / UML |
| `Sidebar` | `components/layout/Sidebar.jsx` | Service + menu; header/footer cố định |

## Token giao diện

Palette quản trị tối giản: canvas lạnh, surface trắng, một accent teal.
Hex chỉ nằm trong `frontend/lms-web/src/theme/tokens.css`. Component lấy class
từ `ui` (`import { ui } from '@/theme'`). Chi tiết: `.interface-design/system.md`.

Signature: `ui.rail` — vạch accent bên trái dòng học viên.

Icon: chỉ Lucide qua `Icon`. Mọi chỗ ấn được dùng `cursor-pointer`.

## Phạm vi hiện tại

Đã có khung app, trang `Danh sách học viên` (`GET /student/api/students`) và
trang `Upload media` (`POST /media/api/media`). POST không tự gọi khi mở
trang; Reset khôi phục default field và xóa kết quả, không tự upload.
Khóa học và scheduler sẽ thêm theo cùng pattern: `api` + `slice` + `hook` +
`pages/<domain>/components`.

Notification Batch hiện có hai menu độc lập: tạo batch (`POST`) và theo dõi
batch (`GET`). Trang theo dõi lưu counters, loading/error và trạng thái pause
trong Redux; hook chỉ polling lại sau 3 giây khi status chưa terminal. Polling
dừng ở `COMPLETED`, `PARTIAL_FAILED`, `FAILED` hoặc khi quản trị viên pause;
nút Tiếp tục gọi GET ngay, không thay đổi Worker nền. Khi batch kết thúc có lỗi,
trang đọc `failed-items` để hiển thị `studentId`, retry và lỗi cuối.
