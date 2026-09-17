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

export interface SessionContextValue {
  session: SessionState | null
  login: (session: SessionState) => void
  logout: () => void
}

export const SessionContext = createContext<SessionContextValue | undefined>(undefined)
