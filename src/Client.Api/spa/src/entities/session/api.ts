import { httpClient } from '../../shared/api'

export { refreshSession } from '../../shared/api'

export async function logoutSession(): Promise<void> {
  await httpClient.post('/auth/logout')
}
