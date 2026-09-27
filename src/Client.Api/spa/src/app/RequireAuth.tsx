import { Navigate } from 'react-router-dom'
import type { ReactNode } from 'react'
import { useSession } from '../entities/session'

function RequireAuth({ children }: { children: ReactNode }) {
  const { status } = useSession()

  if (status === 'resolving') {
    return <p>Loading…</p>
  }

  if (status === 'anonymous') {
    return <Navigate to="/login" replace />
  }

  return children
}

export default RequireAuth
