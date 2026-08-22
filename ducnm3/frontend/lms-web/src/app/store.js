import { configureStore } from '@reduxjs/toolkit'
import { attachHttpInterceptors } from '@/api/httpInterceptors'
import { mediaReducer, uploadMedia } from '@/features/media/mediaSlice'
import { mediaJobsReducer } from '@/features/media/mediaJobsSlice'
import { directUploadReducer } from '@/features/media/directUploadSlice'
import { mediaThumbnailReducer } from '@/features/media/mediaThumbnailSlice'
import { mediaLibraryReducer } from '@/features/media/mediaLibrarySlice'
import { studentsReducer } from '@/features/students/studentsSlice'
import { coursesReducer } from '@/features/courses/coursesSlice'
import { notificationBatchesReducer } from '@/features/notifications/notificationBatchesSlice'
import { toastsReducer } from '@/features/toasts/toastsSlice'
import { studentLearningReducer } from '@/features/studentLearning/studentLearningSlice'
import { dashboardReducer } from '@/features/dashboard/dashboardSlice'

export const store = configureStore({
  reducer: {
    students: studentsReducer,
    courses: coursesReducer,
    media: mediaReducer,
    mediaJobs: mediaJobsReducer,
    directUpload: directUploadReducer,
    mediaThumbnail: mediaThumbnailReducer,
    mediaLibrary: mediaLibraryReducer,
    notificationBatches: notificationBatchesReducer,
    toasts: toastsReducer,
    studentLearning: studentLearningReducer,
    dashboard: dashboardReducer,
  },
  middleware: (getDefaultMiddleware) =>
    getDefaultMiddleware({
      serializableCheck: {
        ignoredActions: [
          uploadMedia.pending.type,
          uploadMedia.fulfilled.type,
          uploadMedia.rejected.type,
        ],
        ignoredActionPaths: ['meta.arg.file'],
      },
    }),
})

attachHttpInterceptors(store)
