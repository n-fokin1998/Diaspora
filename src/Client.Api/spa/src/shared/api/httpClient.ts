import axios from 'axios'
import { getAccessToken, setAccessToken } from './authToken'
import type { AuthApiResponse } from './types'

export const httpClient = axios.create({ baseURL: '/api', withCredentials: true })

httpClient.interceptors.request.use((config) => {
  const token = getAccessToken()
  if (token) {
    config.headers.set('Authorization', `Bearer ${token}`)
  }
  return config
})

let refreshPromise: Promise<AuthApiResponse | null> | null = null

// Single-flight: every caller (the mount-time silent refresh and the 401 interceptor below)
// shares one in-flight request, since the refresh token rotates on every use — two concurrent
// calls would otherwise race to rotate the same token.
export function refreshSession(): Promise<AuthApiResponse | null> {
  refreshPromise ??= httpClient
    .post<AuthApiResponse>('/auth/refresh')
    .then((response) => {
      setAccessToken(response.data.accessToken)
      return response.data
    })
    .catch(() => {
      setAccessToken(null)
      return null
    })
    .finally(() => {
      refreshPromise = null
    })

  return refreshPromise
}

httpClient.interceptors.response.use(
  (response) => response,
  async (error) => {
    const originalRequest = error.config
    const isRefreshCall = originalRequest?.url === '/auth/refresh'

    if (error.response?.status === 401 && originalRequest && !originalRequest._retry && !isRefreshCall) {
      originalRequest._retry = true

      const refreshed = await refreshSession()
      if (refreshed) {
        originalRequest.headers.set('Authorization', `Bearer ${refreshed.accessToken}`)
        return httpClient(originalRequest)
      }
    }

    return Promise.reject(error)
  },
)
