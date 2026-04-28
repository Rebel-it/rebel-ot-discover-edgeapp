import type { AuthObject } from '../models/AuthObject'
import { apiBaseUrl } from './apiBaseUrl'

export async function login(request: AuthObject): Promise<Response> {
  return fetch(`${apiBaseUrl}/api/Authentication/login`, {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
    },
    body: JSON.stringify(request),
  })
}