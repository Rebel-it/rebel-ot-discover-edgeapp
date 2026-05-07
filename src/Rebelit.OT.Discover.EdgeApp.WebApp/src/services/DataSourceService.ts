import type { SourceObject } from '../models/SourceObject'
import { httpClient } from './httpClient'

export function createSource(request: SourceObject): Promise<void> {
  return httpClient.post('/IxonSettings/datasource', request)
}