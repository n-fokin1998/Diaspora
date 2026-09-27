import { useCallback, useEffect, useMemo, useState, type ReactNode } from 'react'
import { setAccessToken } from '../../shared/api'
import { logoutSession, refreshSession } from './api'
import { SessionContext, type SessionState, type SessionStatus } from './context'

export function SessionProvider({ children }: { children: ReactNode }) {
  const [session, setSession] = useState<SessionState | null>(null)
  const [status, setStatus] = useState<SessionStatus>('resolving')

  const login = useCallback((next: SessionState) => {
    setSession(next)
    setStatus('authenticated')
  }, [])

  const logout = useCallback(async () => {
    try {
      await logoutSession()
    } finally {
      setSession(null)
      setStatus('anonymous')
    }
  }, [])

  useEffect(() => {
    setAccessToken(session?.accessToken ?? null)
  }, [session])

  useEffect(() => {
    let cancelled = false

    refreshSession().then((response) => {
      if (cancelled) {
        return
      }

      if (response) {
        setSession({
          user: response.user,
          accessToken: response.accessToken,
          expiresAtUtc: response.expiresAtUtc,
        })
        setStatus('authenticated')
      } else {
        setSession(null)
        setStatus('anonymous')
      }
    })

    return () => {
      cancelled = true
    }
  }, [])

  const value = useMemo(() => ({ session, status, login, logout }), [session, status, login, logout])

  return <SessionContext.Provider value={value}>{children}</SessionContext.Provider>
}
