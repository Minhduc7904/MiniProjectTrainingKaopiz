import { configureStore } from '@reduxjs/toolkit'
import { attachHttpInterceptors } from '@/api/httpInterceptors'
import { mediaReducer, uploadMedia } from '@/features/media/mediaSlice'
import { studentsReducer } from '@/features/students/studentsSlice'
import { notificationBatchesReducer } from '@/features/notifications/notificationBatchesSlice'
import { toastsReducer } from '@/features/toasts/toastsSlice'

export const store = configureStore({
  reducer: {
    students: studentsReducer,
    media: mediaReducer,
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
