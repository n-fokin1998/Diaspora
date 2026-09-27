import { httpClient, type AuthApiResponse } from '../../shared/api'

export interface LoginPayload {
  email: string
  password: string
}

export async function loginUser(payload: LoginPayload): Promise<AuthApiResponse> {
  const response = await httpClient.post<AuthApiResponse>('/auth/login', payload)
  return response.data
}
