# Student Learning Trail UI Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Deliver an independent, responsive Student Learning Trail visual system and dashboard while preserving every Admin UI behaviour and Student identity route.

**Architecture:** The current shared Admin UI and teal theme move under explicit `admin` namespaces without changing their class values. A new Student theme and primitives live in independent `student` namespaces. Student pages compose only Student primitives; their session rules remain in the existing auth/storage modules, and the initial dashboard renders a meaningful no-data state rather than invented course values.

**Tech Stack:** React 19, React Router, Tailwind CSS v4, Vitest, Testing Library, Lucide.

---

## Planned file structure

- Create: `frontend/lms-web/src/theme/admin/tokens.css`, `frontend/lms-web/src/theme/admin/ui.js`, `frontend/lms-web/src/theme/admin/index.js`
- Create: `frontend/lms-web/src/theme/student/tokens.css`, `frontend/lms-web/src/theme/student/ui.js`, `frontend/lms-web/src/theme/student/index.js`
- Move: all present `frontend/lms-web/src/components/ui/*` Admin files to `frontend/lms-web/src/components/ui/admin/`
- Create: `frontend/lms-web/src/components/ui/student/StudentButton.jsx`, `StudentInput.jsx`, `StudentCard.jsx`, `StudentShell.jsx`, `ProgressRing.jsx`, `LessonTrail.jsx`, `StudentEmptyState.jsx`, `StudentLoadingState.jsx`, and `index.js`
- Create: `frontend/lms-web/src/pages/student-auth/components/StudentAuthLayout.jsx` and `StudentDashboard.jsx`
- Modify: `frontend/lms-web/src/index.css`, `frontend/lms-web/src/theme/index.js`, Admin imports, and `frontend/lms-web/src/pages/student-auth/StudentAuthPages.jsx`
- Create tests: `frontend/lms-web/src/pages/student-auth/StudentAuthPages.test.jsx`, `frontend/lms-web/src/components/ui/student/LessonTrail.test.jsx`
- Create test: `frontend/lms-web/src/components/ui/admin/AdminUiNamespace.test.jsx`
- Modify docs: `.interface-design/system.md` and `docs/superpowers/specs/2026-08-21-student-learning-trail-design.md` only if implementation exposes a design discrepancy.

### Task 1: Lock Student visual composition with failing tests

**Files:**
- Create: `frontend/lms-web/src/pages/student-auth/StudentAuthPages.test.jsx`
- Create: `frontend/lms-web/src/components/ui/student/LessonTrail.test.jsx`

- [ ] **Step 1: Write the Student dashboard and trail tests**

```jsx
it('shows a learning-focused empty dashboard after a valid Student session', () => {
  render(<StudentHomePage />)
  expect(screen.getByRole('heading', { name: 'Tiếp tục hành trình học' })).toBeVisible()
  expect(screen.getByText('Bạn chưa có khóa học đang học.')).toBeVisible()
  expect(screen.getByRole('link', { name: 'Đăng xuất' })).toBeVisible()
})

it('announces lesson state with text as well as colour', () => {
  render(<LessonTrail items={[{ title: 'Khởi động', state: 'completed' }]} />)
  expect(screen.getByText('Đã hoàn thành')).toBeVisible()
})
```

- [ ] **Step 2: Run the new tests to verify RED**

Run: `npm test -- StudentAuthPages.test.jsx LessonTrail.test.jsx --run`

Expected: FAIL because `LessonTrail`, the Learning Trail heading, and Student
dashboard composition do not exist yet.

- [ ] **Step 3: Commit the test specification**

Do not commit unless the user explicitly requests a commit. Keep the failing
tests in the working tree for the next task.

### Task 2: Separate the Admin visual namespace without changing Admin appearance

**Files:**
- Create: `frontend/lms-web/src/theme/admin/tokens.css`
- Create: `frontend/lms-web/src/theme/admin/ui.js`
- Create: `frontend/lms-web/src/theme/admin/index.js`
- Create: `frontend/lms-web/src/components/ui/admin/AdminUiNamespace.test.jsx`
- Modify: `frontend/lms-web/src/index.css`, `frontend/lms-web/src/theme/index.js`
- Move: `frontend/lms-web/src/components/ui/{ActivityDiagram,ApiField,ApiToast,ApiToastHost,Button,ConfirmModal,Dropdown,EmptyState,Field,Icon,JsonView,LoadingState,PageHeader,Pagination,Skeleton,Spinner,StatusBadge,Tabs,controlStyles,dropdownPlacement}.{jsx,js}` to `frontend/lms-web/src/components/ui/admin/`
- Modify: every Admin/layout/media/notification/course/student-list import currently matching `@/components/ui/` or `@/theme`

- [ ] **Step 1: Define the expected Admin namespace contract in a test**

```jsx
import { Button } from '@/components/ui/admin/Button'
import { adminUi } from '@/theme/admin'

it('keeps the Admin primary button bound to the Admin theme', () => {
  render(<Button>Save</Button>)
  expect(screen.getByRole('button', { name: 'Save' })).toHaveClass(
    ...adminUi.buttonPrimary.split(' '),
  )
})
```

- [ ] **Step 2: Run the Admin namespace test to verify RED**

Run: `npm test -- AdminUiNamespace.test.jsx --run`

Expected: FAIL because Admin files and `adminUi` are not yet exported.

- [ ] **Step 3: Move Admin theme and primitives, preserving their implementation**

Copy the existing `tokens.css` values to `theme/admin/tokens.css`, rename the
exported map `ui` to `adminUi`, and move all current UI files beneath
`components/ui/admin/`. Update their internal imports to `adminUi` and update
all non-Student page imports to the new Admin paths. `index.css` imports only
the Admin tokens globally; Student tokens are scoped by Student component
classes and must not override Admin CSS variables.

- [ ] **Step 4: Run Admin tests and lint to verify GREEN**

Run: `npm test -- AdminUiNamespace.test.jsx --run && npm run lint`

Expected: PASS with no unresolved `@/components/ui/` or `ui` imports in Admin
code.

- [ ] **Step 5: Commit the isolated Admin migration**

Do not commit unless the user explicitly requests a commit.

### Task 3: Build the Student token namespace and foundational primitives

**Files:**
- Create: `frontend/lms-web/src/theme/student/tokens.css`
- Create: `frontend/lms-web/src/theme/student/ui.js`
- Create: `frontend/lms-web/src/theme/student/index.js`
- Create: `frontend/lms-web/src/components/ui/student/{StudentButton,StudentInput,StudentCard,StudentShell,StudentLoadingState,StudentEmptyState,index}.jsx`
- Modify: `frontend/lms-web/src/index.css`

- [ ] **Step 1: Extend the failing test for Student affordances**

```jsx
it('renders Student controls with accessible labels and Student theme classes', () => {
  render(<StudentInput id="email" label="Email" value="" onChange={() => {}} />)
  expect(screen.getByLabelText('Email')).toHaveClass('student-control')
})
```

- [ ] **Step 2: Run the Student UI tests to verify RED**

Run: `npm test -- StudentAuthPages.test.jsx LessonTrail.test.jsx --run`

Expected: FAIL because Student primitives and semantic Student classes do not
exist.

- [ ] **Step 3: Implement Student tokens and primitives**

Implement the indigo/lavender semantic palette from the approved spec in
`theme/student/tokens.css`, including `student-canvas`, `student-surface`,
`student-ink`, `student-primary`, `student-bookmark`, and
`student-complete`. `studentUi` provides Tailwind class strings for all Student
controls. Use native `button`, `input`, `label`, `main`, and `nav` elements;
all buttons have a 44px minimum target and visible focus. Use Lucide through a
Student wrapper or direct existing icon constants, never emoji.

- [ ] **Step 4: Add scoped motion utilities**

Add Student-only classes for entry, card reveal, press feedback, and progress
movement. They animate only `transform` and `opacity`; use 120–240ms values and
the approved custom easing. Add a `prefers-reduced-motion` rule that disables
all Student transitions and animations without changing Admin skeleton motion.

- [ ] **Step 5: Run the Student UI tests to verify GREEN**

Run: `npm test -- StudentAuthPages.test.jsx LessonTrail.test.jsx --run`

Expected: PASS.

### Task 4: Implement Lesson Trail and Student dashboard/auth compositions

**Files:**
- Create: `frontend/lms-web/src/components/ui/student/ProgressRing.jsx`
- Create: `frontend/lms-web/src/components/ui/student/LessonTrail.jsx`
- Create: `frontend/lms-web/src/pages/student-auth/components/StudentAuthLayout.jsx`
- Create: `frontend/lms-web/src/pages/student-auth/components/StudentDashboard.jsx`
- Modify: `frontend/lms-web/src/pages/student-auth/StudentAuthPages.jsx`

- [ ] **Step 1: Add a failing route-state test**

```jsx
it('keeps the existing loading redirect when no Student actor is stored', async () => {
  render(<MemoryRouter><StudentLoadingPage /></MemoryRouter>)
  await waitFor(() => expect(mockNavigate).toHaveBeenCalledWith('/student/login', { replace: true }))
})
```

- [ ] **Step 2: Run the Student page tests to verify RED**

Run: `npm test -- StudentAuthPages.test.jsx LessonTrail.test.jsx --run`

Expected: FAIL until the new composition preserves the route-state contract.

- [ ] **Step 3: Compose pages exclusively from Student UI**

Replace direct Admin `Button`/`ui` imports in `StudentAuthPages.jsx` with
Student components. `StudentHomePage` renders the Learning Trail dashboard with
an intentional no-course state and a shell logout link. Login and register keep
their existing API calls and redirects; loading keeps the existing `me`
verification and storage-clear behaviour. Do not create course/progress data or
new Axios calls.

- [ ] **Step 4: Implement accessible Lesson Trail semantics**

`LessonTrail` receives items as `{ title, state }`, renders an ordered list,
and announces `Đã hoàn thành`, `Đang học`, or `Bài tiếp theo` visibly. It uses
an icon plus text so state is never communicated by color alone. The empty
dashboard uses no trail items and explains that a course must be enrolled.

- [ ] **Step 5: Run targeted tests to verify GREEN**

Run: `npm test -- StudentAuthPages.test.jsx LessonTrail.test.jsx --run`

Expected: PASS while retaining the existing login, register, loading and logout
route semantics.

### Task 5: Verify production behaviour and record Student design decisions

**Files:**
- Modify: `.interface-design/system.md`
- Modify: `docs/superpowers/specs/2026-08-21-student-learning-trail-design.md` only when implementation differs from approved decisions

- [ ] **Step 1: Run the frontend test suite**

Run: `npm test -- --run`

Expected: PASS.

- [ ] **Step 2: Run lint and production build**

Run: `npm run lint && npm run build`

Expected: PASS; no unresolved Admin/Student import path and no hard-coded
Student palette values in JSX.

- [ ] **Step 3: Perform responsive and motion checks**

Inspect `/student/register`, `/student/login`, `/student/loading`, and
`/student/home` at 375px and desktop width. Confirm focus rings, no horizontal
overflow, 44px touch targets, reduced-motion behaviour, and no Admin teal
surface in Student views. Confirm a representative `/admin/student/students`
screen retains its existing Admin UI.

- [ ] **Step 4: Save the Student pattern system**

Append a Student section to `.interface-design/system.md` with the Learning
Trail direction, palette, spacing, shadow strategy, component ownership, and
motion constraints. Keep the existing Admin decisions unchanged.

- [ ] **Step 5: Commit verified changes**

Do not commit unless the user explicitly requests a commit.
