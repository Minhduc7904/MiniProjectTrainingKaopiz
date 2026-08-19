import { GET_STUDENTS_ACTIVITY } from '@/constants/activities/getStudents'
import { API_ROUTES } from '@/constants/apiRoutes'

export const GET_COURSES_ACTIVITY = { ...GET_STUDENTS_ACTIVITY, id: 'getCourses', title: 'Liệt kê Khóa học', path: API_ROUTES.courses.list }
export const EXPORT_COURSES_ACTIVITY = { ...GET_STUDENTS_ACTIVITY, id: 'exportCourses', title: 'Xuất CSV Khóa học', path: API_ROUTES.courses.export }
