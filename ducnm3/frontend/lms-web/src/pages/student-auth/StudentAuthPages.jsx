import { useEffect, useState } from 'react'
import { Link, Navigate, useNavigate, useSearchParams } from 'react-router-dom'
import { studentAuthApi } from '@/api/studentAuthApi'
import { clearStudentActor, readStudentActor, writeStudentActor } from '@/auth/studentAuthStorage'
import { verifyStudentSession } from '@/auth/studentSession'
import { APP_ROUTES } from '@/constants/appRoutes'
import { Button } from '@/components/ui/Button'
import { ui } from '@/theme'

function AuthLayout({ title, children }) {
  return <main className={`flex min-h-full items-center justify-center p-6 ${ui.page}`}><section className={`w-full max-w-md rounded-lg border p-6 ${ui.card}`}>{title && <h1 className={`font-display text-2xl font-semibold ${ui.title}`}>{title}</h1>}{children}</section></main>
}

function AuthError({ error }) {
  return error ? <p className={`mt-4 text-sm ${ui.danger}`}>{error.message ?? 'Request failed.'}</p> : null
}

export function StudentRegisterPage() {
  const navigate = useNavigate(); const [email, setEmail] = useState(''); const [displayName, setDisplayName] = useState(''); const [error, setError] = useState(null); const [loading, setLoading] = useState(false)
  async function submit(event) { event.preventDefault(); setLoading(true); setError(null); try { const data = await studentAuthApi.register({ email, displayName }); writeStudentActor(data.id); navigate(APP_ROUTES.studentLoading, { replace: true }) } catch (reason) { setError(reason) } finally { setLoading(false) } }
  return <AuthLayout title="Đăng ký học viên"><form className="mt-6 grid gap-4" onSubmit={submit}><label className={ui.label}>Email<input className={ui.input} value={email} onChange={(event) => setEmail(event.target.value)} required type="email" /></label><label className={ui.label}>Tên hiển thị<input className={ui.input} value={displayName} onChange={(event) => setDisplayName(event.target.value)} required /></label><Button disabled={loading} type="submit">Đăng ký</Button></form><AuthError error={error} /><p className={`mt-4 text-sm ${ui.caption}`}>Đã có ID? <Link className="cursor-pointer underline" to={APP_ROUTES.studentLogin}>Đăng nhập</Link></p></AuthLayout>
}

export function StudentLoginPage() {
  const navigate = useNavigate(); const [id, setId] = useState(''); const [error, setError] = useState(null); const [loading, setLoading] = useState(false)
  async function submit(event) { event.preventDefault(); setLoading(true); setError(null); try { const data = await studentAuthApi.login({ id }); writeStudentActor(data.id); navigate(APP_ROUTES.studentLoading, { replace: true }) } catch (reason) { setError(reason) } finally { setLoading(false) } }
  return <AuthLayout title="Đăng nhập học viên"><form className="mt-6 grid gap-4" onSubmit={submit}><label className={ui.label}>Student ID<input className={ui.input} value={id} onChange={(event) => setId(event.target.value)} required /></label><Button disabled={loading} type="submit">Đăng nhập</Button></form><AuthError error={error} /><p className={`mt-4 text-sm ${ui.caption}`}>Chưa có tài khoản? <Link className="cursor-pointer underline" to={APP_ROUTES.studentRegister}>Đăng ký</Link></p></AuthLayout>
}

export function StudentLoadingPage() {
  const navigate = useNavigate(); const [params] = useSearchParams(); const [error, setError] = useState(null)
  useEffect(() => { const actor = readStudentActor(); if (!actor) { navigate(APP_ROUTES.studentLogin, { replace: true }); return } let active = true; studentAuthApi.me().then(() => { verifyStudentSession(actor.id); if (active) navigate(params.get('returnTo') || APP_ROUTES.studentHome, { replace: true }) }).catch((reason) => { clearStudentActor(); if (active) { setError(reason); navigate(APP_ROUTES.studentLogin, { replace: true }) } }); return () => { active = false } }, [navigate, params])
  return <AuthLayout title="Đang kiểm tra phiên học viên"><p className={ui.caption}>{error ? 'Phiên không hợp lệ.' : 'Vui lòng chờ...'}</p></AuthLayout>
}

export function StudentLogoutPage() { clearStudentActor(); return <Navigate to={APP_ROUTES.studentLogin} replace /> }

export function StudentHomePage() { return <AuthLayout title="Khu vực học viên"><p className={ui.caption}>Bạn đã đăng nhập thành công.</p><Link className="mt-4 inline-block cursor-pointer underline" to={APP_ROUTES.studentLogout}>Đăng xuất</Link></AuthLayout> }
