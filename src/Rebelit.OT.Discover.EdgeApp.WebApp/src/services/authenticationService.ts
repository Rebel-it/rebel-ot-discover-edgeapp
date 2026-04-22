export interface LoginRequest {
  username: string
  password: string
  otpCode: string
  applicationID: string
}

export async function login(request: LoginRequest): Promise<Response> {
  return fetch('https://localhost:61411/api/Authentication/login', {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
    },
    body: JSON.stringify(request),
  })
}