import type { PlcAuthObject } from '../models/PlcAuthObject.ts'
import { httpClient } from './httpClient'

export function connectToPlc(request: PlcAuthObject): Promise<void> {
  return httpClient.post('/Plc/connect', request)
}