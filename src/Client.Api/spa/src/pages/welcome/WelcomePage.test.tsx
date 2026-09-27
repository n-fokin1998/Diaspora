import { render, screen } from '@testing-library/react'
import { MemoryRouter } from 'react-router-dom'
import { describe, expect, it } from 'vitest'
import WelcomePage from './WelcomePage'

function renderWelcomePage() {
  render(
    <MemoryRouter>
      <WelcomePage />
    </MemoryRouter>,
  )
}

describe('Welcome page', () => {
  it('shows the Diaspora logo and a description of what the product does', () => {
    renderWelcomePage()

    expect(screen.getByRole('img', { name: /diaspora logo/i })).toBeInTheDocument()
    expect(
      screen.getByText(/find friends and .*? activities/i)
    ).toBeInTheDocument()
  })

  it('links Sign Up to the registration page', () => {
    renderWelcomePage()

    const signUpLink = screen.getByRole('link', { name: /sign up/i })
    expect(signUpLink).toHaveAttribute('href', '/register')
  })

  it('links Sign In to the login page', () => {
    renderWelcomePage()

    const signInLink = screen.getByRole('link', { name: /sign in/i })
    expect(signInLink).toHaveAttribute('href', '/login')
  })
})
