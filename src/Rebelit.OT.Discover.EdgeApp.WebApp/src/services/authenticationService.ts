import type { AuthObject } from '../models/AuthObject'
import { apiBaseUrl } from './apiBaseUrl'

export async function validateCredentials(request: AuthObject): Promise<Response> {
  return fetch(`${apiBaseUrl}/Authentication/validate`, {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
    },
    body: JSON.stringify(request),
  })
}