import type { ApiResponseObject } from '../models/ApiResponseObject'
import type { SourceObject } from '../models/SourceObject'
import { httpClient } from './httpClient'

export function createSource(request: SourceObject): Promise<ApiResponseObject<void>> {
  return httpClient.post('/IxonSettings/datasource', request)
}