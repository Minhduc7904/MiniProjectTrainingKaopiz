# lms-web

SPA React của Mini LMS. Kiến trúc và quy tắc folder nằm ở
[`docs/architecture/frontend.md`](../../docs/architecture/frontend.md).

```bash
cp .env.example .env
npm install
npm run dev
```

Gateway mặc định: `http://localhost:5100`.

## Deploy Vercel

Vercel project phải đặt Root Directory là `frontend/lms-web`. File `vercel.json`
rewrite mọi URL SPA về `index.html`; nhờ đó reload hoặc mở trực tiếp các route
như `/admin/student/students` vẫn để React Router render đúng trang.
