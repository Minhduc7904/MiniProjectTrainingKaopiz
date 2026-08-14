import { GET_STUDENTS_ACTIVITY } from '@/constants/activities/getStudents'

export const ACTIVITIES = {
  [GET_STUDENTS_ACTIVITY.id]: GET_STUDENTS_ACTIVITY,
}

export function getActivityById(activityId) {
  return ACTIVITIES[activityId] ?? null
}
