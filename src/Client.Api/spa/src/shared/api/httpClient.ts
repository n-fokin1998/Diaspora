import axios from 'axios'
import { getAccessToken } from './authToken'

export const httpClient = axios.create({ baseURL: '/api' })

httpClient.interceptors.request.use((config) => {
  const token = getAccessToken()
  if (token) {
    config.headers.set('Authorization', `Bearer ${token}`)
  }
  return config
})
