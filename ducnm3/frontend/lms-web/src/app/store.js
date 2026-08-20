import { configureStore } from '@reduxjs/toolkit'
import { attachHttpInterceptors } from '@/api/httpInterceptors'
import { mediaReducer, uploadMedia } from '@/features/media/mediaSlice'
import { directUploadReducer } from '@/features/media/directUploadSlice'
import { mediaThumbnailReducer } from '@/features/media/mediaThumbnailSlice'
import { mediaLibraryReducer } from '@/features/media/mediaLibrarySlice'
import { studentsReducer } from '@/features/students/studentsSlice'
import { coursesReducer } from '@/features/courses/coursesSlice'
import { notificationBatchesReducer } from '@/features/notifications/notificationBatchesSlice'
import { toastsReducer } from '@/features/toasts/toastsSlice'

export const store = configureStore({
  reducer: {
    students: studentsReducer,
    courses: coursesReducer,
    media: mediaReducer,
    directUpload: directUploadReducer,
    mediaThumbnail: mediaThumbnailReducer,
    mediaLibrary: mediaLibraryReducer,
    notificationBatches: notificationBatchesReducer,
    toasts: toastsReducer,
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
