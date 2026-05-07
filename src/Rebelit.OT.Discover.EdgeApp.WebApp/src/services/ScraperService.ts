import type { ApiResponseObject } from '../models/ApiResponseObject'
import { httpClient } from './httpClient'

export function synchronizeVariables(): Promise<ApiResponseObject<void>> {
  return httpClient.post('/scraper/variables')
}