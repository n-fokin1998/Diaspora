import { fireEvent, render, screen, waitFor } from '@testing-library/react'
import { MemoryRouter, Route, Routes } from 'react-router-dom'
import { beforeEach, describe, expect, it, vi } from 'vitest'
import { SessionProvider } from '../../entities/session'
import HomePage from '../../pages/home/HomePage'
import LoginForm from './LoginForm'
import { loginUser } from './api'

vi.mock('./api', () => ({
  loginUser: vi.fn(),
}))

function renderLoginForm() {
  render(
    <SessionProvider>
      <MemoryRouter initialEntries={['/login']}>
        <Routes>
          <Route path="/login" element={<LoginForm />} />
          <Route path="/home" element={<div>home-marker</div>} />
        </Routes>
      </MemoryRouter>
    </SessionProvider>,
  )
}

function fillForm(email: string, password: string) {
  fireEvent.change(screen.getByLabelText(/^email$/i), { target: { value: email } })
  fireEvent.change(screen.getByLabelText(/^password$/i), { target: { value: password } })
}

describe('LoginForm', () => {
  beforeEach(() => {
    vi.mocked(loginUser).mockReset()
  })

  it('stores the session and navigates to /home on a valid submit', async () => {
    vi.mocked(loginUser).mockResolvedValue({
      accessToken: 'token-123',
      expiresAtUtc: '2026-09-15T13:00:00Z',
      user: { id: '1', email: 'jane@example.com', firstName: 'Jane', lastName: 'Doe' },
    })

    renderLoginForm()
    fillForm('jane@example.com', 'Str0ngPass1')
    fireEvent.click(screen.getByRole('button', { name: /sign in/i }))

    await waitFor(() => expect(screen.getByText('home-marker')).toBeInTheDocument())
  })

  it('shows one generic error for wrong credentials, not a field-specific one', async () => {
    vi.mocked(loginUser).mockRejectedValue({
      isAxiosError: true,
      response: { status: 401, data: {} },
    })

    renderLoginForm()
    fillForm('jane@example.com', 'WrongPassword1')
    fireEvent.click(screen.getByRole('button', { name: /sign in/i }))

    expect(await screen.findByRole('alert')).toHaveTextContent(/email or password is incorrect/i)
    expect(screen.queryByText('home-marker')).not.toBeInTheDocument()
  })

  it('logging out from Home clears the session and returns to /login', async () => {
    vi.mocked(loginUser).mockResolvedValue({
      accessToken: 'token-123',
      expiresAtUtc: '2026-09-15T13:00:00Z',
      user: { id: '1', email: 'jane@example.com', firstName: 'Jane', lastName: 'Doe' },
    })

    render(
      <SessionProvider>
        <MemoryRouter initialEntries={['/login']}>
          <Routes>
            <Route path="/login" element={<LoginForm />} />
            <Route path="/home" element={<HomePage />} />
          </Routes>
        </MemoryRouter>
      </SessionProvider>,
    )

    fillForm('jane@example.com', 'Str0ngPass1')
    fireEvent.click(screen.getByRole('button', { name: /sign in/i }))
    await waitFor(() => expect(screen.getByRole('button', { name: /log out/i })).toBeInTheDocument())

    fireEvent.click(screen.getByRole('button', { name: /log out/i }))

    await waitFor(() => expect(screen.getByLabelText(/^email$/i)).toBeInTheDocument())
  })
})
