import { useState, type FormEvent } from 'react'
import '../../shared/ui/authForm.css'
import { validateRegisterForm } from '../../shared/lib/validation'
import { useRegisterUser } from './useRegisterUser'

function RegisterForm() {
  const { submit, fieldErrors: serverFieldErrors, conflictError, isSubmitting } = useRegisterUser()
  const [clientErrors, setClientErrors] = useState<Record<string, string>>({})

  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [confirmPassword, setConfirmPassword] = useState('')
  const [firstName, setFirstName] = useState('')
  const [lastName, setLastName] = useState('')
  const [dateOfBirth, setDateOfBirth] = useState('')
  const [location, setLocation] = useState('')

  function errorFor(field: string): string | undefined {
    return clientErrors[field] ?? serverFieldErrors[field]?.[0]
  }

  const emailError = errorFor('email')
  const passwordError = errorFor('password')
  const confirmPasswordError = errorFor('confirmPassword')
  const firstNameError = errorFor('firstName')
  const lastNameError = errorFor('lastName')
  const dateOfBirthError = errorFor('dateOfBirth')
  const locationError = errorFor('location')

  function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()

    const values = { email, password, confirmPassword, firstName, lastName, dateOfBirth, location }
    const errors = validateRegisterForm(values)
    setClientErrors(errors)

    if (Object.keys(errors).length > 0) {
      return
    }

    void submit(values)
  }

  return (
    <form className="auth-form" onSubmit={handleSubmit} noValidate>
      {conflictError && (
        <p className="auth-banner-error" role="alert">
          {conflictError}
        </p>
      )}

      <div className="auth-form-row">
        <div className="auth-field">
          <label htmlFor="firstName">First name</label>
          <input
            id="firstName"
            name="firstName"
            autoComplete="given-name"
            value={firstName}
            onChange={(e) => setFirstName(e.target.value)}
            aria-invalid={Boolean(firstNameError)}
          />
          {firstNameError && <span className="auth-field-error">{firstNameError}</span>}
        </div>
        <div className="auth-field">
          <label htmlFor="lastName">Last name</label>
          <input
            id="lastName"
            name="lastName"
            autoComplete="family-name"
            value={lastName}
            onChange={(e) => setLastName(e.target.value)}
            aria-invalid={Boolean(lastNameError)}
          />
          {lastNameError && <span className="auth-field-error">{lastNameError}</span>}
        </div>
      </div>

      <div className="auth-field">
        <label htmlFor="email">Email</label>
        <input
          id="email"
          name="email"
          type="email"
          autoComplete="email"
          value={email}
          onChange={(e) => setEmail(e.target.value)}
          aria-invalid={Boolean(emailError)}
        />
        {emailError && <span className="auth-field-error">{emailError}</span>}
      </div>

      <div className="auth-field">
        <label htmlFor="password">Password</label>
        <input
          id="password"
          name="password"
          type="password"
          autoComplete="new-password"
          value={password}
          onChange={(e) => setPassword(e.target.value)}
          aria-invalid={Boolean(passwordError)}
        />
        {passwordError && <span className="auth-field-error">{passwordError}</span>}
      </div>

      <div className="auth-field">
        <label htmlFor="confirmPassword">Confirm password</label>
        <input
          id="confirmPassword"
          name="confirmPassword"
          type="password"
          autoComplete="new-password"
          value={confirmPassword}
          onChange={(e) => setConfirmPassword(e.target.value)}
          aria-invalid={Boolean(confirmPasswordError)}
        />
        {confirmPasswordError && <span className="auth-field-error">{confirmPasswordError}</span>}
      </div>

      <div className="auth-field">
        <label htmlFor="dateOfBirth">Date of birth</label>
        <input
          id="dateOfBirth"
          name="dateOfBirth"
          type="date"
          autoComplete="bday"
          value={dateOfBirth}
          onChange={(e) => setDateOfBirth(e.target.value)}
          aria-invalid={Boolean(dateOfBirthError)}
        />
        {dateOfBirthError && <span className="auth-field-error">{dateOfBirthError}</span>}
      </div>

      <div className="auth-field">
        <label htmlFor="location">Location</label>
        <input
          id="location"
          name="location"
          autoComplete="address-level2"
          value={location}
          onChange={(e) => setLocation(e.target.value)}
          aria-invalid={Boolean(locationError)}
        />
        {locationError && <span className="auth-field-error">{locationError}</span>}
      </div>

      <button type="submit" className="auth-submit" disabled={isSubmitting}>
        {isSubmitting ? 'Creating account…' : 'Create account'}
      </button>
    </form>
  )
}

export default RegisterForm
