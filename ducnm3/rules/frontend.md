# Frontend rules

- Frontend code sống trong `frontend/lms-web/`.
- Đọc `docs/architecture/frontend.md` và `.agents/skills/interface-design/`
  trước khi thêm page hoặc visual component.
- Mỗi page tách: `Page` composition, `pages/<name>/components/` riêng trang,
  `components/ui/` dùng chung.
- Gọi API chỉ trong `src/api/` và hook `src/hooks/`. Page không dùng Axios.
- `data`, `pagination`, `loading`, `success`, `error` của API list/detail phải
  nằm trong Redux, không giữ local state cho các field này.
- Mọi HTTP request qua `httpClient` phải hiện toast toàn cục; không gắn toast
  từng page. Pending progress = `VITE_API_TIMEOUT_MS`.
- Không hard-code path, header, status, query name. Dùng `src/constants/`.
- Cấu hình môi trường chỉ qua `VITE_*` trong `.env`; không commit `.env`.
- Token CSS chỉ khai báo trong `frontend/lms-web/src/theme/tokens.css`.
  Component không viết hex, không dùng `bg-gray-*` / `text-slate-*`, không
  gắn `bg-accent` trực tiếp. Mọi class màu lấy từ `ui` trong
  `frontend/lms-web/src/theme/ui.js` (`import { ui } from '@/theme'`).
- Mọi chỗ ấn được (`button`, `a`, `NavLink`, option dropdown, label `for`)
  phải có `cursor-pointer`; disabled dùng `cursor-not-allowed`.
- Icon chỉ dùng Lucide (`lucide-react`) qua `components/ui/Icon.jsx`. Không
  dùng emoji, glyph Unicode, hay bộ icon khác.
