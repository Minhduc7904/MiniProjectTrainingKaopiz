import { GET_STUDENTS_ACTIVITY } from '@/constants/activities/getStudents'
import { POST_MEDIA_ACTIVITY } from '@/constants/activities/postMedia'
import { POST_MEDIA_DIRECT_ACTIVITY } from '@/constants/activities/postMediaDirect'
import { POST_NOTIFICATION_BATCH_ACTIVITY } from '@/constants/activities/postNotificationBatch'
import { GET_NOTIFICATION_BATCH_ACTIVITY } from '@/constants/activities/getNotificationBatch'
import { GET_NOTIFICATION_BATCHES_ACTIVITY } from '@/constants/activities/getNotificationBatches'

export const ACTIVITIES = {
  [GET_STUDENTS_ACTIVITY.id]: GET_STUDENTS_ACTIVITY,
  [POST_MEDIA_ACTIVITY.id]: POST_MEDIA_ACTIVITY,
  [POST_MEDIA_DIRECT_ACTIVITY.id]: POST_MEDIA_DIRECT_ACTIVITY,
  [POST_NOTIFICATION_BATCH_ACTIVITY.id]: POST_NOTIFICATION_BATCH_ACTIVITY,
  [GET_NOTIFICATION_BATCH_ACTIVITY.id]: GET_NOTIFICATION_BATCH_ACTIVITY,
  [GET_NOTIFICATION_BATCHES_ACTIVITY.id]: GET_NOTIFICATION_BATCHES_ACTIVITY,
}

export function getActivityById(activityId) {
  return ACTIVITIES[activityId] ?? null
}
