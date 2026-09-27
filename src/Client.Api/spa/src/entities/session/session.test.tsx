import { fireEvent, render, screen, waitFor } from '@testing-library/react'
import { beforeEach, describe, expect, it, vi } from 'vitest'
import { logoutSession, refreshSession } from './api'
import { SessionProvider, useSession } from './index'

vi.mock('./api', () => ({
  refreshSession: vi.fn(),
  logoutSession: vi.fn(),
}))

function StatusProbe() {
  const { status, session, logout } = useSession()

  if (status === 'resolving') {
    return <p>resolving</p>
  }

  if (status === 'anonymous') {
    return <p>anonymous</p>
  }

  return (
    <div>
      <p>authenticated as {session?.user.email}</p>
      <button type="button" onClick={() => void logout()}>
        Log out
      </button>
    </div>
  )
}

describe('SessionProvider', () => {
  beforeEach(() => {
    vi.mocked(refreshSession).mockReset()
    vi.mocked(logoutSession).mockReset()
  })

  it('resolves to authenticated when the silent refresh on mount succeeds', async () => {
    vi.mocked(refreshSession).mockResolvedValue({
      accessToken: 'token-123',
      expiresAtUtc: '2026-09-22T13:00:00Z',
      user: { id: '1', email: 'jane@example.com', firstName: 'Jane', lastName: 'Doe' },
    })

    render(
      <SessionProvider>
        <StatusProbe />
      </SessionProvider>,
    )

    expect(screen.getByText('resolving')).toBeInTheDocument()
    await waitFor(() => expect(screen.getByText(/authenticated as jane@example.com/)).toBeInTheDocument())
  })

  it('resolves to anonymous when the silent refresh on mount fails (no valid session cookie)', async () => {
    vi.mocked(refreshSession).mockResolvedValue(null)

    render(
      <SessionProvider>
        <StatusProbe />
      </SessionProvider>,
    )

    await waitFor(() => expect(screen.getByText('anonymous')).toBeInTheDocument())
  })

  it('logging out calls the logout endpoint before clearing the session', async () => {
    vi.mocked(refreshSession).mockResolvedValue({
      accessToken: 'token-123',
      expiresAtUtc: '2026-09-22T13:00:00Z',
      user: { id: '1', email: 'jane@example.com', firstName: 'Jane', lastName: 'Doe' },
    })
    vi.mocked(logoutSession).mockResolvedValue(undefined)

    render(
      <SessionProvider>
        <StatusProbe />
      </SessionProvider>,
    )

    await waitFor(() => expect(screen.getByText(/authenticated as/)).toBeInTheDocument())

    fireEvent.click(screen.getByRole('button', { name: /log out/i }))

    await waitFor(() => expect(screen.getByText('anonymous')).toBeInTheDocument())
    expect(logoutSession).toHaveBeenCalledTimes(1)
  })
})
