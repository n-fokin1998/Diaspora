// Mirrors the validation rules enforced server-side in RegisterCommandHandler and the
// User entity's MaxNameLength/MaxLocationLength/MinAgeYears constants (data-model.md's
// Validation Summary), so a visitor sees the same feedback immediately instead of only
// after a round trip to the server.
const MAX_NAME_LENGTH = 100
const MAX_LOCATION_LENGTH = 200
const MIN_AGE_YEARS = 13
const EMAIL_PATTERN = /^[^@\s]+@[^@\s]+\.[^@\s]+$/

export function validateEmail(email: string): string | undefined {
  const trimmed = email.trim()
  if (!trimmed) {
    return 'Email is required.'
  }
  if (!EMAIL_PATTERN.test(trimmed)) {
    return 'Enter a valid email address.'
  }
  return undefined
}

export function validatePassword(password: string): string | undefined {
  if (password.length < 8 || !/[A-Za-z]/.test(password) || !/[0-9]/.test(password)) {
    return 'Password must be at least 8 characters and include both letters and numbers.'
  }
  return undefined
}

export function validateConfirmPassword(password: string, confirmPassword: string): string | undefined {
  if (password !== confirmPassword) {
    return 'Passwords do not match.'
  }
  return undefined
}

export function validateRequiredText(value: string, label: string, maxLength: number): string | undefined {
  const trimmed = value.trim()
  if (!trimmed) {
    return `${label} is required.`
  }
  if (trimmed.length > maxLength) {
    return `${label} must be ${maxLength} characters or fewer.`
  }
  return undefined
}

export function validateFirstName(value: string): string | undefined {
  return validateRequiredText(value, 'First name', MAX_NAME_LENGTH)
}

export function validateLastName(value: string): string | undefined {
  return validateRequiredText(value, 'Last name', MAX_NAME_LENGTH)
}

export function validateLocation(value: string): string | undefined {
  return validateRequiredText(value, 'Location', MAX_LOCATION_LENGTH)
}

export function validateDateOfBirth(dateOfBirth: string): string | undefined {
  if (!dateOfBirth) {
    return 'Date of birth is required.'
  }

  const dob = new Date(`${dateOfBirth}T00:00:00`)
  if (Number.isNaN(dob.getTime())) {
    return 'Enter a valid date.'
  }

  const today = new Date()
  if (dob.getTime() > today.getTime()) {
    return 'Date of birth cannot be in the future.'
  }

  let age = today.getFullYear() - dob.getFullYear()
  const hasHadBirthdayThisYear =
    today.getMonth() > dob.getMonth() ||
    (today.getMonth() === dob.getMonth() && today.getDate() >= dob.getDate())
  if (!hasHadBirthdayThisYear) {
    age--
  }

  if (age < MIN_AGE_YEARS) {
    return `You must be at least ${MIN_AGE_YEARS} years old to register.`
  }

  return undefined
}

export interface RegisterFormValues {
  email: string
  password: string
  confirmPassword: string
  firstName: string
  lastName: string
  dateOfBirth: string
  location: string
}

export function validateRegisterForm(values: RegisterFormValues): Record<string, string> {
  const errors: Record<string, string> = {}

  const email = validateEmail(values.email)
  if (email) errors.email = email

  const password = validatePassword(values.password)
  if (password) errors.password = password

  const confirmPassword = validateConfirmPassword(values.password, values.confirmPassword)
  if (confirmPassword) errors.confirmPassword = confirmPassword

  const firstName = validateFirstName(values.firstName)
  if (firstName) errors.firstName = firstName

  const lastName = validateLastName(values.lastName)
  if (lastName) errors.lastName = lastName

  const dateOfBirth = validateDateOfBirth(values.dateOfBirth)
  if (dateOfBirth) errors.dateOfBirth = dateOfBirth

  const location = validateLocation(values.location)
  if (location) errors.location = location

  return errors
}
