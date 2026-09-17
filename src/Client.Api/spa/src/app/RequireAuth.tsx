import { Navigate } from 'react-router-dom'
import type { ReactNode } from 'react'
import { useSession } from '../entities/session'

function RequireAuth({ children }: { children: ReactNode }) {
  const { session } = useSession()

  if (!session) {
    return <Navigate to="/login" replace />
  }

  return children
}

export default RequireAuth
