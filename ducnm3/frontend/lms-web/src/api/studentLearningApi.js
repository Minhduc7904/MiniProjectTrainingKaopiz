import { studentHttpClient } from '@/api/studentHttpClient'
import { unwrapEnvelope } from '@/api/unwrapEnvelope'
import { API_ROUTES } from '@/constants/apiRoutes'

export async function fetchStudentEnrollmentsRequest(query = {}) {
  const params = Object.fromEntries(Object.entries(query).filter(([, value]) => value != null && value !== ''))
  return unwrapEnvelope(await studentHttpClient.get(API_ROUTES.studentLearning.enrollments, { params }))
}

export async function fetchStudentCourseCatalogRequest(query = {}) {
  const params = Object.fromEntries(Object.entries(query).filter(([, value]) => value != null && value !== ''))
  return unwrapEnvelope(await studentHttpClient.get(API_ROUTES.studentLearning.catalog, { params }))
}

export async function enrollStudentInCourseRequest(courseId) {
  return unwrapEnvelope(await studentHttpClient.post(API_ROUTES.studentLearning.enroll(courseId)))
}

export async function fetchStudentEnrollmentDetailRequest(courseId) {
  return unwrapEnvelope(await studentHttpClient.get(API_ROUTES.studentLearning.enrollmentDetail(courseId)))
}

export async function fetchStudentLessonDetailRequest(courseId, lessonId) {
  return unwrapEnvelope(await studentHttpClient.get(API_ROUTES.studentLearning.lessonDetail(courseId, lessonId)))
}

export async function completeStudentLessonRequest(courseId, lessonId) {
  return unwrapEnvelope(await studentHttpClient.post(API_ROUTES.studentLearning.completeLesson(courseId, lessonId)))
}

export async function fetchMyCourseProgressRequest(courseId) {
  return unwrapEnvelope(await studentHttpClient.get(API_ROUTES.studentLearning.progress(courseId)))
}

export async function fetchStudentMediaContentRequest(contentUrl, options = {}) {
  return studentHttpClient.get(contentUrl, { responseType: 'blob', signal: options.signal }).then((response) => response.data)
}
