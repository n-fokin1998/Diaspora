import { render, screen } from '@testing-library/react'
import { describe, expect, it } from 'vitest'
import App from './App'

describe('Welcome page', () => {
  it('shows the Diaspora logo and a description of what the product does', () => {
    render(<App />)

    expect(screen.getByRole('img', { name: /diaspora logo/i })).toBeInTheDocument()
    expect(
      screen.getByText(/find friends and .*? activities/i)
    ).toBeInTheDocument()
  })

  it('shows a sign-up control that is not wired to any navigation yet', () => {
    render(<App />)

    const signUpButton = screen.getByRole('button', { name: /sign up/i })
    expect(signUpButton).toBeInTheDocument()
    expect(signUpButton.tagName).toBe('BUTTON')
    expect(signUpButton).not.toHaveAttribute('href')
  })

  it('shows a sign-in control that is not wired to any navigation yet', () => {
    render(<App />)

    const signInButton = screen.getByRole('button', { name: /sign in/i })
    expect(signInButton).toBeInTheDocument()
    expect(signInButton.tagName).toBe('BUTTON')
    expect(signInButton).not.toHaveAttribute('href')
  })
})
