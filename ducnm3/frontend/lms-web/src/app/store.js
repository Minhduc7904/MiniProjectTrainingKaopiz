import { configureStore } from '@reduxjs/toolkit'
import { attachHttpInterceptors } from '@/api/httpInterceptors'
import { mediaReducer, uploadMedia } from '@/features/media/mediaSlice'
import { studentsReducer } from '@/features/students/studentsSlice'
import { toastsReducer } from '@/features/toasts/toastsSlice'

export const store = configureStore({
  reducer: {
    students: studentsReducer,
    media: mediaReducer,
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
