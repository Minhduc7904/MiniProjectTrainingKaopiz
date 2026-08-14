import { Provider } from 'react-redux'
import { BrowserRouter } from 'react-router-dom'
import { store } from '@/app/store'
import { AppRouter } from '@/app/router'
import { ApiToastHost } from '@/components/ui/ApiToastHost'

export function AppProviders() {
  return (
    <Provider store={store}>
      <BrowserRouter>
        <AppRouter />
        <ApiToastHost />
      </BrowserRouter>
    </Provider>
  )
}
