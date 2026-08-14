# Design System

## Direction

**Personality:** Quiet admin — bảng điểm buổi chiều, không phải giấy kraft,
không phải console slate/blue generic.

**Foundation:** canvas lạnh nhạt, surface trắng, một accent teal.

**Depth:** borders-only, đường kẻ `line` thấp tương phản.

## Tokens

Hex sống **một chỗ**: `frontend/lms-web/src/theme/tokens.css`.
Class semantic sống **một chỗ**: `frontend/lms-web/src/theme/ui.js`.

Component import `{ ui } from '@/theme'`. Không hard-code màu.

### Spacing
Base: 4px
Scale: 4, 8, 12, 16, 24, 32

### Colors

```
--canvas          #f4f5f7   nền trang + sidebar
--surface         #ffffff   card, bảng, toast, dropdown
--surface-muted   #eef0f3   header bảng, hover, badge fallback
--control         #f3f4f6   input inset
--line            #e2e5ea   border
--fg              #111318   chữ chính
--fg-muted        #5a6170   chữ phụ
--fg-subtle       #8a909c   nhãn / meta
--accent          #156b63   brand / success / action
--accent-hover    #115751
--accent-soft     mix 12%   nền nav active, badge active
--on-accent       #ffffff   chữ trên nút primary
--danger          #c0392b
--warning         #a15c12
```

60% canvas + surface, 30% chữ/line, 10% accent. Warning/danger chỉ cho status.

### Radius
Scale: 6px, 8px

### Typography
Display + body: Source Sans 3
Caption: 11–12px tracked uppercase
Ratio: 1.25 from 14px body

### Icons
Lucide only, `strokeWidth` 1.75, size 16 / 18 / 22. Wrap with `Icon`.

## Patterns

### Cursor
Mọi control ấn được: `cursor-pointer`. Disabled: `cursor-not-allowed`.

### Button Primary
- Height: 36px; icon-only 36×36
- Background: accent; text: on-accent

### Pagination
Icon Lucide first/prev/next/last + Dropdown pageSize. Class màu từ `ui`.

### Register row
`ui.rail` — vạch accent 3px trái dòng sổ.

### Toast API
`ui.toast*` — rail + bar + icon theo phase.

### Workbench
Trang API: trái Input (tab Mẫu dropdown và tab Thủ công đủ field, ghi rõ
nullable), phải Output. Output có tab JSON, Xem, và UML Activity Diagram
(một menu = một API). Success tô luồng chính; lỗi tô nhánh Fault đúng error
code.

### Sidebar
`fixed` full height. Header (brand) và footer không cuộn. Chỉ danh sách menu cuộn.
Dropdown service đổi bộ menu.

## Decisions

| Decision | Rationale | Date |
|----------|-----------|------|
| Cool canvas + one teal accent | Admin tối giản, dễ đọc; tránh giấy nâu và gray SaaS | 2026-08-14 |
| Sidebar cùng canvas | Không tách “hai thế giới” sidebar/content | 2026-08-14 |
| Hex only in tokens.css | Đổi palette một file; component không biết hex | 2026-08-14 |
| Semantic `ui` map | Tránh copy `bg-accent` rải rác, lệch theme | 2026-08-14 |
| Borders-only depth | Roster dense; shadow làm nặng | 2026-08-14 |
| Lucide for every icon | One stroke language | 2026-08-14 |
| cursor-pointer on every clickable | Affordance rõ; disabled = not-allowed | 2026-08-14 |
