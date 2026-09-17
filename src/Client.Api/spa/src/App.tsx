import AppRouter from './app/AppRouter'
import { SessionProvider } from './entities/session'

function App() {
  return (
    <SessionProvider>
      <AppRouter />
    </SessionProvider>
  )
}

export default App
