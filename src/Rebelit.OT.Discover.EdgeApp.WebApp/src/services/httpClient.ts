import { apiBaseUrl } from './apiBaseUrl'
import { loadIxonAuthenticationHeaders } from './sessionStorageService'

function buildHeaders(): Record<string, string> {
  const headers: Record<string, string> = {
    'Content-Type': 'application/json',
  }

  const auth = loadIxonAuthenticationHeaders();
  
  if (auth) {
    headers['Api-Application'] = auth.ApplicationId;
    headers['Api-Access-Token'] = auth.AccessToken;

    if (auth.CompanyId) {
      headers['Api-Company-Id'] = auth.CompanyId;
    }

    if (auth.AgentId) {
      headers['Api-Agent-Id'] = auth.AgentId;
    }
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

  return undefined as unknown as T
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

  put<TResponse = void, TBody = unknown>(path: string, body?: TBody): Promise<TResponse> {
    return fetch(`${apiBaseUrl}${path}`, {
      method: 'PUT',
      headers: buildHeaders(),
      body: body !== undefined ? JSON.stringify(body) : undefined,
    }).then(handleResponse<TResponse>)
  }
}
