import { useState } from 'react'
import axios from 'axios'
import { useNavigate } from 'react-router-dom'
import { useSession } from '../../entities/session'
import type { ApiValidationProblem, FieldErrors } from '../../shared/api'
import { registerUser, type RegisterPayload } from './api'

export function useRegisterUser() {
  const { login } = useSession()
  const navigate = useNavigate()
  const [fieldErrors, setFieldErrors] = useState<FieldErrors>({})
  const [conflictError, setConflictError] = useState<string | null>(null)
  const [isSubmitting, setIsSubmitting] = useState(false)

  async function submit(payload: RegisterPayload) {
    setIsSubmitting(true)
    setFieldErrors({})
    setConflictError(null)

    try {
      const response = await registerUser(payload)
      login({
        user: response.user,
        accessToken: response.accessToken,
        expiresAtUtc: response.expiresAtUtc,
      })
      navigate('/home')
    } catch (error) {
      if (axios.isAxiosError(error) && error.response) {
        if (error.response.status === 409) {
          setConflictError('An account with this email address already exists.')
        } else if (error.response.status === 400) {
          const problem = error.response.data as ApiValidationProblem
          setFieldErrors(problem.errors ?? {})
        } else {
          setConflictError('Something went wrong. Please try again.')
        }
      } else {
        setConflictError('Something went wrong. Please try again.')
      }
    } finally {
      setIsSubmitting(false)
    }
  }

  return { submit, fieldErrors, conflictError, isSubmitting }
}
