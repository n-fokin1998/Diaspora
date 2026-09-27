import '../../shared/ui/authForm.css'
import { RegisterForm } from '../../features/register-user'

function RegisterPage() {
  return (
    <main className="auth-page">
      <h1>Create your account</h1>
      <RegisterForm />
    </main>
  )
}

export default RegisterPage
