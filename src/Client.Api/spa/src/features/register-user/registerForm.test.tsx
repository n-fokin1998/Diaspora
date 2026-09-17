import { fireEvent, render, screen, waitFor } from '@testing-library/react'
import { MemoryRouter, Route, Routes } from 'react-router-dom'
import { beforeEach, describe, expect, it, vi } from 'vitest'
import { SessionProvider } from '../../entities/session'
import RegisterForm from './RegisterForm'
import { registerUser } from './api'

vi.mock('./api', () => ({
  registerUser: vi.fn(),
}))

function renderRegisterForm() {
  render(
    <SessionProvider>
      <MemoryRouter initialEntries={['/register']}>
        <Routes>
          <Route path="/register" element={<RegisterForm />} />
          <Route path="/home" element={<div>home-marker</div>} />
        </Routes>
      </MemoryRouter>
    </SessionProvider>,
  )
}

function fillValidForm() {
  fireEvent.change(screen.getByLabelText(/first name/i), { target: { value: 'Jane' } })
  fireEvent.change(screen.getByLabelText(/last name/i), { target: { value: 'Doe' } })
  fireEvent.change(screen.getByLabelText(/^email$/i), { target: { value: 'jane@example.com' } })
  fireEvent.change(screen.getByLabelText(/^password$/i), { target: { value: 'Str0ngPass1' } })
  fireEvent.change(screen.getByLabelText(/confirm password/i), { target: { value: 'Str0ngPass1' } })
  fireEvent.change(screen.getByLabelText(/date of birth/i), { target: { value: '1998-04-12' } })
  fireEvent.change(screen.getByLabelText(/location/i), { target: { value: 'Berlin' } })
}

describe('RegisterForm', () => {
  beforeEach(() => {
    vi.mocked(registerUser).mockReset()
  })

  it('stores the session and navigates to /home on a valid submit', async () => {
    vi.mocked(registerUser).mockResolvedValue({
      accessToken: 'token-123',
      expiresAtUtc: '2026-09-15T13:00:00Z',
      user: { id: '1', email: 'jane@example.com', firstName: 'Jane', lastName: 'Doe' },
    })

    renderRegisterForm()
    fillValidForm()
    fireEvent.click(screen.getByRole('button', { name: /create account/i }))

    await waitFor(() => expect(screen.getByText('home-marker')).toBeInTheDocument())
  })

  it('shows a duplicate-email error without navigating away', async () => {
    vi.mocked(registerUser).mockRejectedValue({
      isAxiosError: true,
      response: { status: 409, data: {} },
    })

    renderRegisterForm()
    fillValidForm()
    fireEvent.click(screen.getByRole('button', { name: /create account/i }))

    expect(await screen.findByRole('alert')).toHaveTextContent(/already exists/i)
    expect(screen.queryByText('home-marker')).not.toBeInTheDocument()
  })

  it('shows a password-mismatch field error returned by the server', async () => {
    vi.mocked(registerUser).mockRejectedValue({
      isAxiosError: true,
      response: {
        status: 400,
        data: { errors: { confirmPassword: ['Passwords do not match.'] } },
      },
    })

    renderRegisterForm()
    fillValidForm()
    fireEvent.click(screen.getByRole('button', { name: /create account/i }))

    expect(await screen.findByText(/passwords do not match/i)).toBeInTheDocument()
  })
})
