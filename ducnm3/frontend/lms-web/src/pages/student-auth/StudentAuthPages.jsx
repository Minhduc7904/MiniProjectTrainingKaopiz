import { useEffect, useState } from 'react'
import { Link, Navigate, useNavigate, useSearchParams } from 'react-router-dom'
import { studentAuthApi } from '@/api/studentAuthApi'
import { clearStudentActor, readStudentActor, writeStudentActor } from '@/auth/studentAuthStorage'
import { getVerifiedStudentProfile, verifyStudentSession } from '@/auth/studentSession'
import { APP_ROUTES } from '@/constants/appRoutes'
import { StudentButton, StudentInput, StudentLoadingState } from '@/components/ui/student'
import { studentUi } from '@/theme/student'
import { StudentAuthLayout } from './components/StudentAuthLayout'
import { StudentDashboard } from './components/StudentDashboard'
import { StudentProfile } from './components/StudentProfile'
import { StudentCourseCatalog } from './components/StudentCourseCatalog'
import { StudentCourseDetail } from './components/StudentCourseDetail'
import { StudentLearningPage } from './components/StudentLearningPage'

function AuthError({ error }) {
  return error ? <p aria-live="polite" className={`mt-4 ${studentUi.error}`}>{error.message ?? 'Không thể xử lý yêu cầu. Hãy thử lại.'}</p> : null
}

export function StudentRegisterPage() {
  const navigate = useNavigate(); const [email, setEmail] = useState(''); const [displayName, setDisplayName] = useState(''); const [error, setError] = useState(null); const [loading, setLoading] = useState(false)
  async function submit(event) { event.preventDefault(); setLoading(true); setError(null); try { const data = await studentAuthApi.register({ email, displayName }); writeStudentActor(data.id); navigate(APP_ROUTES.studentLoading, { replace: true }) } catch (reason) { setError(reason) } finally { setLoading(false) } }
  return <StudentAuthLayout title="Bắt đầu hành trình học"><form className="mt-8 grid gap-5" onSubmit={submit}><StudentInput autoComplete="email" id="student-email" label="Email" spellCheck={false} value={email} onChange={(event) => setEmail(event.target.value)} required type="email" /><StudentInput autoComplete="nickname" id="student-display-name" label="Tên hiển thị" value={displayName} onChange={(event) => setDisplayName(event.target.value)} required /><StudentButton disabled={loading} type="submit">{loading ? 'Đang tạo tài khoản…' : 'Tạo tài khoản học'}</StudentButton></form><AuthError error={error} /><p className={`mt-5 ${studentUi.caption}`}>Đã có Student ID? <Link className={studentUi.link} to={APP_ROUTES.studentLogin}>Đăng nhập</Link></p></StudentAuthLayout>
}

export function StudentLoginPage() {
  const navigate = useNavigate(); const [id, setId] = useState(''); const [error, setError] = useState(null); const [loading, setLoading] = useState(false)
  async function submit(event) { event.preventDefault(); setLoading(true); setError(null); try { const data = await studentAuthApi.login({ id }); writeStudentActor(data.id); navigate(APP_ROUTES.studentLoading, { replace: true }) } catch (reason) { setError(reason) } finally { setLoading(false) } }
  return <StudentAuthLayout title="Chào mừng trở lại"><form className="mt-8 grid gap-5" onSubmit={submit}><StudentInput autoComplete="username" hint="ID được tạo khi bạn đăng ký." id="student-id" label="Student ID" spellCheck={false} value={id} onChange={(event) => setId(event.target.value)} required /><StudentButton disabled={loading} type="submit">{loading ? 'Đang đăng nhập…' : 'Vào khu vực học'}</StudentButton></form><AuthError error={error} /><p className={`mt-5 ${studentUi.caption}`}>Chưa có tài khoản? <Link className={studentUi.link} to={APP_ROUTES.studentRegister}>Đăng ký ngay</Link></p></StudentAuthLayout>
}

export function StudentLoadingPage() {
  const navigate = useNavigate(); const [params] = useSearchParams()
  useEffect(() => { const actor = readStudentActor(); if (!actor) { navigate(APP_ROUTES.studentLogin, { replace: true }); return } let active = true; studentAuthApi.me().then((student) => { verifyStudentSession(student); if (active) navigate(params.get('returnTo') || APP_ROUTES.studentHome, { replace: true }) }).catch(() => { clearStudentActor(); if (active) navigate(APP_ROUTES.studentLogin, { replace: true }) }); return () => { active = false } }, [navigate, params])
  return <StudentAuthLayout title="Đang chuẩn bị phiên học"><StudentLoadingState title="Chúng tôi đang xác minh không gian học của bạn." /></StudentAuthLayout>
}

export function StudentLogoutPage() { clearStudentActor(); return <Navigate to={APP_ROUTES.studentLogin} replace /> }

export function StudentHomePage() { return <StudentDashboard student={getVerifiedStudentProfile()} /> }

export function StudentProfilePage() { return <StudentProfile student={getVerifiedStudentProfile()} /> }

export function StudentCoursesPage() { return <StudentCourseCatalog student={getVerifiedStudentProfile()} /> }

export function StudentCourseDetailPage() { return <StudentCourseDetail student={getVerifiedStudentProfile()} /> }
export function StudentCourseLearnPage() { return <StudentLearningPage student={getVerifiedStudentProfile()} /> }
