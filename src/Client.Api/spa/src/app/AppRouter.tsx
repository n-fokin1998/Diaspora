import { BrowserRouter, Route, Routes } from 'react-router-dom'
import WelcomePage from '../pages/welcome/WelcomePage'
import HomePage from '../pages/home/HomePage'
import RegisterPage from '../pages/register/RegisterPage'
import LoginPage from '../pages/login/LoginPage'
import RequireAuth from './RequireAuth'

function AppRouter() {
  return (
    <BrowserRouter>
      <Routes>
        <Route path="/" element={<WelcomePage />} />
        <Route path="/register" element={<RegisterPage />} />
        <Route path="/login" element={<LoginPage />} />
        <Route
          path="/home"
          element={
            <RequireAuth>
              <HomePage />
            </RequireAuth>
          }
        />
      </Routes>
    </BrowserRouter>
  )
}

export default AppRouter
