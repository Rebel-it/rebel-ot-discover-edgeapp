import type { ApiResponseObject } from '../models/ApiResponseObject';
import { apiBaseUrl } from './apiBaseUrl'
import { loadIxonAuthenticationHeaders } from './sessionStorageService'

function buildHeaders(): Record<string, string> {
  const headers: Record<string, string> = {
    'Content-Type': 'application/json',
  }

  const auth = loadIxonAuthenticationHeaders();
  
  if (auth) {
    console.log('Adding authentication headers:', auth);
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

async function handleResponse<T>(response: Response): Promise<ApiResponseObject<T | undefined>> {
  if (!response.ok) {
    const text = await response.text()
    throw new Error(text || `${response.status} ${response.statusText}`)
  }

  const contentType = response.headers.get('content-type')
  if (contentType?.includes('application/json')) {
    return response.json() as Promise<ApiResponseObject<T | undefined>>
  }

  return undefined as unknown as ApiResponseObject<undefined>
}

export const httpClient = {
  get<T>(path: string): Promise<ApiResponseObject<T | undefined>> {
    return fetch(`${apiBaseUrl}${path}`, {
      method: 'GET',
      headers: buildHeaders(),
    }).then(handleResponse<T>)
  },

  post<TResponse = void, TBody = unknown>(path: string, body?: TBody): Promise<ApiResponseObject<TResponse | undefined>> {
    return fetch(`${apiBaseUrl}${path}`, {
      method: 'POST',
      headers: buildHeaders(),
      body: body !== undefined ? JSON.stringify(body) : undefined,
    }).then(handleResponse<TResponse>)
  },
}
