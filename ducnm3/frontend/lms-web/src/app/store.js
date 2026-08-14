import { configureStore } from '@reduxjs/toolkit'
import { attachHttpInterceptors } from '@/api/httpInterceptors'
import { studentsReducer } from '@/features/students/studentsSlice'
import { toastsReducer } from '@/features/toasts/toastsSlice'

export const store = configureStore({
  reducer: {
    students: studentsReducer,
    toasts: toastsReducer,
  },
})

attachHttpInterceptors(store)
