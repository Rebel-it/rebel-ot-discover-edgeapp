import { httpClient } from './httpClient'

export function synchronizeVariables(): Promise<void> {
  return httpClient.post('/scraper/variables')
}