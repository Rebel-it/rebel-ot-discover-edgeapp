import type { AuthObject } from '../models/AuthObject'

export async function login(request: AuthObject): Promise<Response> {
  return fetch('https://localhost:61411/api/Authentication/login', {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
    },
    body: JSON.stringify(request),
  })
}