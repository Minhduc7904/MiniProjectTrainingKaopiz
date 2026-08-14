# Template Frontend API page

Thay `<Domain>`, `<resource>`, `<ServiceId>`, `<httpMethod>`.
Mẫu copy: `pages/students/` + `hooks/students/` + `features/students/`.

## Phiếu chốt

```text
Service id (sidebar):
Menu label:
UI route:
Gateway method + path:
API doc:
Business flow:
Query/body fields (nullable / required / default / allowlist):
Pagination: offset | cursor | none
Error codes cho UML Fault:
```

## Checklist

- [ ] Đọc SKILL + reference + API doc + business-flow + `rules/frontend.md`
      + `interface-design`.
- [ ] Constants: routes, params, input fields, default query, activity, copy.
- [ ] `api/<domain>Api.js` dùng `httpClient` + `unwrapEnvelope`.
- [ ] Slice Redux list/detail; hook `load` + `reset`.
- [ ] Store + router + `SERVICES` menu `activityId` + Sidebar icon.
- [ ] Page: Workbench, InputPanel (Mẫu, Thủ công, Reset), OutputPanel
      (JSON, Xem, UML).
- [ ] Theme `ui`, Lucide `Icon`, `cursor-pointer`.
- [ ] `npm run lint` && `npm run build`.

## Menu

`constants/appRoutes.js`:

```js
{
  id: SERVICE_IDS.<service>,
  label: '<Service>',
  icon: '<service>',
  menus: [
    {
      to: APP_ROUTES.<resource>,
      label: '<Tên menu>',
      icon: '<menu>',
      ready: true,
      activityId: '<activityId>',
    },
  ],
}
```

Map icon Lucide trong `components/layout/Sidebar.jsx`.

## Default query + fields

`constants/inputs/<api>.js` — xem `constants/inputs/getStudents.js`.

## Hook

```js
const setQuery = (query) => dispatch(setXQuery(query))
const load = (query) => dispatch(fetchX(query))
const reset = () => dispatch(fetchX(DEFAULT_QUERY))
```

Tab Mẫu và Thủ công cùng `query` / `setQuery`. Không draft local.

## Page skeleton

```jsx
<Workbench
  input={
    <InputPanel
      actions={
        <Button variant="ghost" disabled={loading} onClick={reset}>
          <Icon icon={RotateCcw} />
          {UI_LABELS.reset}
        </Button>
      }
      guided={/* PageHeader + filters onChange={load} */}
      manual={/* ManualForm query onChange={setQuery} onSubmit={load} */}
    />
  }
  output={
    <OutputPanel json={envelope} activity={ACTIVITY} run={{ loading, success, error }}>
      {/* EmptyState / TableSkeleton / Table */}
    </OutputPanel>
  }
/>
```

Envelope JSON:

```js
error
  ? { data: null, error, meta: { traceId, query } }
  : { data, meta: { pagination, traceId, query }, success }
```

## UML

Copy `constants/activities/getStudents.js`: `lanes`, `nodes`, `edges`,
`pendingPath`, `successPath`, `failByCode` từ business-flow và error code BE.

## Không làm

- Axios trong page/component UI.
- List state local (`useState` cho `data`/`error`/`loading`).
- Draft Input local từng tab (Mẫu/Thủ công phải cùng `query`).
- Toast trong page.
- Palette/hex rải component.
- Hai API trên một page.
- Placeholder menu `ready: false` khi đã có API.
