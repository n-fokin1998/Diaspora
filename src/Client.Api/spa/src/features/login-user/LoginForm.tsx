import { useState, type FormEvent } from 'react'
import '../../shared/ui/authForm.css'
import { useLoginUser } from './useLoginUser'

function LoginForm() {
  const { submit, error, isSubmitting } = useLoginUser()
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')

  function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    void submit({ email, password })
  }

  return (
    <form className="auth-form" onSubmit={handleSubmit} noValidate>
      {error && (
        <p className="auth-banner-error" role="alert">
          {error}
        </p>
      )}

      <div className="auth-field">
        <label htmlFor="email">Email</label>
        <input
          id="email"
          name="email"
          type="email"
          autoComplete="email"
          value={email}
          onChange={(e) => setEmail(e.target.value)}
          aria-invalid={Boolean(error)}
        />
      </div>

      <div className="auth-field">
        <label htmlFor="password">Password</label>
        <input
          id="password"
          name="password"
          type="password"
          autoComplete="current-password"
          value={password}
          onChange={(e) => setPassword(e.target.value)}
          aria-invalid={Boolean(error)}
        />
      </div>

      <button type="submit" className="auth-submit" disabled={isSubmitting}>
        {isSubmitting ? 'Signing in…' : 'Sign in'}
      </button>
    </form>
  )
}

export default LoginForm
