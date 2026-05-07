import type { ApiResponseObject } from '../models/ApiResponseObject.ts'
import type { PlcAuthObject } from '../models/PlcAuthObject.ts'
import { httpClient } from './httpClient'

export function connectToPlc(request: PlcAuthObject): Promise<ApiResponseObject<void>> {
  return httpClient.post('/Plc/connect', request)
}