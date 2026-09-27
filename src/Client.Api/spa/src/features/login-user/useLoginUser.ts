import { useState } from 'react'
import axios from 'axios'
import { useNavigate } from 'react-router-dom'
import { useSession } from '../../entities/session'
import { loginUser, type LoginPayload } from './api'

export function useLoginUser() {
  const { login } = useSession()
  const navigate = useNavigate()
  const [error, setError] = useState<string | null>(null)
  const [isSubmitting, setIsSubmitting] = useState(false)

  async function submit(payload: LoginPayload) {
    setIsSubmitting(true)
    setError(null)

    try {
      const response = await loginUser(payload)
      login({
        user: response.user,
        accessToken: response.accessToken,
        expiresAtUtc: response.expiresAtUtc,
      })
      navigate('/home')
    } catch (caught) {
      if (axios.isAxiosError(caught) && caught.response) {
        if (caught.response.status === 401) {
          setError('The email or password is incorrect.')
        } else {
          setError('Something went wrong. Please try again.')
        }
      } else {
        setError('Something went wrong. Please try again.')
      }
    } finally {
      setIsSubmitting(false)
    }
  }

  return { submit, error, isSubmitting }
}
