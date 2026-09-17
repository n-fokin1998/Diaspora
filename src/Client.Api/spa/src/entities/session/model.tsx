import { useCallback, useEffect, useMemo, useState, type ReactNode } from 'react'
import { setAccessToken } from '../../shared/api'
import { SessionContext, type SessionState } from './context'

export function SessionProvider({ children }: { children: ReactNode }) {
  const [session, setSession] = useState<SessionState | null>(null)

  const login = useCallback((next: SessionState) => setSession(next), [])
  const logout = useCallback(() => setSession(null), [])

  useEffect(() => {
    setAccessToken(session?.accessToken ?? null)
  }, [session])

  const value = useMemo(() => ({ session, login, logout }), [session, login, logout])

  return <SessionContext.Provider value={value}>{children}</SessionContext.Provider>
}
