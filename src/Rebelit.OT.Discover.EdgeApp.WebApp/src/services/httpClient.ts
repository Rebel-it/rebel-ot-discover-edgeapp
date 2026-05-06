import { apiBaseUrl } from './apiBaseUrl'
import { loadAuthFromSession } from './sessionStorageService'

function buildHeaders(): Record<string, string> {
  const headers: Record<string, string> = {
    'Content-Type': 'application/json',
  }

  const auth = loadAuthFromSession()
  if (auth) {
    headers['Api-application'] = auth.APIapplicationID
    headers['Api-Access-Token'] = auth.AccessToken
  }

  return headers
}

async function handleResponse<T>(response: Response): Promise<T> {
  if (!response.ok) {
    const text = await response.text()
    throw new Error(text || `${response.status} ${response.statusText}`)
  }

  const contentType = response.headers.get('content-type')
  if (contentType?.includes('application/json')) {
    return response.json() as Promise<T>
  }

  return undefined as T
}

export const httpClient = {
  get<T>(path: string): Promise<T> {
    return fetch(`${apiBaseUrl}${path}`, {
      method: 'GET',
      headers: buildHeaders(),
    }).then(handleResponse<T>)
  },

  post<TResponse = void, TBody = unknown>(path: string, body?: TBody): Promise<TResponse> {
    return fetch(`${apiBaseUrl}${path}`, {
      method: 'POST',
      headers: buildHeaders(),
      body: body !== undefined ? JSON.stringify(body) : undefined,
    }).then(handleResponse<TResponse>)
  },
}
