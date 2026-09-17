import { describe, expect, it } from 'vitest'
import { validateRegisterForm, type RegisterFormValues } from './validation'

function validValues(overrides: Partial<RegisterFormValues> = {}): RegisterFormValues {
  return {
    email: 'jane@example.com',
    password: 'Str0ngPass1',
    confirmPassword: 'Str0ngPass1',
    firstName: 'Jane',
    lastName: 'Doe',
    dateOfBirth: '1998-04-12',
    location: 'Berlin, Germany',
    ...overrides,
  }
}

describe('validateRegisterForm', () => {
  it('returns no errors for fully valid input', () => {
    expect(validateRegisterForm(validValues())).toEqual({})
  })

  it('flags a malformed email', () => {
    const errors = validateRegisterForm(validValues({ email: 'not-an-email' }))
    expect(errors.email).toMatch(/valid email/i)
  })

  it('flags a password that is too short or missing a digit', () => {
    const errors = validateRegisterForm(validValues({ password: 'short', confirmPassword: 'short' }))
    expect(errors.password).toMatch(/at least 8 characters/i)
  })

  it('flags a confirm-password mismatch', () => {
    const errors = validateRegisterForm(validValues({ confirmPassword: 'Different1' }))
    expect(errors.confirmPassword).toMatch(/do not match/i)
  })

  it('flags an empty first or last name', () => {
    const errors = validateRegisterForm(validValues({ firstName: '  ', lastName: '' }))
    expect(errors.firstName).toMatch(/required/i)
    expect(errors.lastName).toMatch(/required/i)
  })

  it('flags an empty location', () => {
    const errors = validateRegisterForm(validValues({ location: '   ' }))
    expect(errors.location).toMatch(/required/i)
  })

  it('flags a future date of birth', () => {
    const futureYear = new Date().getFullYear() + 1
    const errors = validateRegisterForm(validValues({ dateOfBirth: `${futureYear}-01-01` }))
    expect(errors.dateOfBirth).toMatch(/future/i)
  })

  it('flags a date of birth under the minimum age', () => {
    const tooYoungYear = new Date().getFullYear() - 5
    const errors = validateRegisterForm(validValues({ dateOfBirth: `${tooYoungYear}-01-01` }))
    expect(errors.dateOfBirth).toMatch(/at least 13 years old/i)
  })
})
