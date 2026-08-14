import { GET_STUDENTS_ACTIVITY } from '@/constants/activities/getStudents'
import { POST_MEDIA_ACTIVITY } from '@/constants/activities/postMedia'
import { POST_NOTIFICATION_BATCH_ACTIVITY } from '@/constants/activities/postNotificationBatch'
import { GET_NOTIFICATION_BATCH_ACTIVITY } from '@/constants/activities/getNotificationBatch'

export const ACTIVITIES = {
  [GET_STUDENTS_ACTIVITY.id]: GET_STUDENTS_ACTIVITY,
  [POST_MEDIA_ACTIVITY.id]: POST_MEDIA_ACTIVITY,
  [POST_NOTIFICATION_BATCH_ACTIVITY.id]: POST_NOTIFICATION_BATCH_ACTIVITY,
  [GET_NOTIFICATION_BATCH_ACTIVITY.id]: GET_NOTIFICATION_BATCH_ACTIVITY,
}

export function getActivityById(activityId) {
  return ACTIVITIES[activityId] ?? null
}
