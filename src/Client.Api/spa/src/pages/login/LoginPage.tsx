import '../../shared/ui/authForm.css'
import { LoginForm } from '../../features/login-user'

function LoginPage() {
  return (
    <main className="auth-page">
      <h1>Sign in</h1>
      <LoginForm />
    </main>
  )
}

export default LoginPage
