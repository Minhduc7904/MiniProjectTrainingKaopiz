import { GET_STUDENTS_ACTIVITY } from '@/constants/activities/getStudents'
import { POST_MEDIA_ACTIVITY } from '@/constants/activities/postMedia'

export const ACTIVITIES = {
  [GET_STUDENTS_ACTIVITY.id]: GET_STUDENTS_ACTIVITY,
  [POST_MEDIA_ACTIVITY.id]: POST_MEDIA_ACTIVITY,
}

export function getActivityById(activityId) {
  return ACTIVITIES[activityId] ?? null
}
