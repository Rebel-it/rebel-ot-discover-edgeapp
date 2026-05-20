import { httpClient } from './httpClient'

export async function getVariables(): Promise<string[]> {
    return httpClient.get<string[]>('/scraper/variables')
}

export function synchronizeVariables(): Promise<void> {
  return httpClient.post('/scraper/variables')
}