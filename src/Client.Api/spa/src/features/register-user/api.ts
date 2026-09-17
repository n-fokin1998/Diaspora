import { httpClient, type AuthApiResponse } from '../../shared/api'

export interface RegisterPayload {
  email: string
  password: string
  confirmPassword: string
  firstName: string
  lastName: string
  dateOfBirth: string
  location: string
}

export async function registerUser(payload: RegisterPayload): Promise<AuthApiResponse> {
  const response = await httpClient.post<AuthApiResponse>('/auth/register', payload)
  return response.data
}
