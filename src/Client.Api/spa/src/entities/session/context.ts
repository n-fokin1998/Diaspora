import { createContext } from 'react'

export interface SessionUser {
  id: string
  email: string
  firstName: string
  lastName: string
}

export interface SessionState {
  user: SessionUser
  accessToken: string
  expiresAtUtc: string
}

export type SessionStatus = 'resolving' | 'authenticated' | 'anonymous'

export interface SessionContextValue {
  session: SessionState | null
  status: SessionStatus
  login: (session: SessionState) => void
  logout: () => Promise<void>
}

export const SessionContext = createContext<SessionContextValue | undefined>(undefined)
