import type { SourceObject } from '../models/SourceObject'
import { httpClient } from './httpClient'

export function createSource(request: SourceObject): Promise<string> {
  return httpClient.post('/IxonSettings/datasource', request)
}