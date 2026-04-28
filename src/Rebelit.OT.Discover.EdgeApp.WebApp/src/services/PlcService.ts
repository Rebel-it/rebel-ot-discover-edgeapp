import type { PlcAuthObject } from '../models/PlcAuthObject.ts'
import { apiBaseUrl } from './apiBaseUrl'

export async function connectToPlc(request: PlcAuthObject): Promise<Response> {
    return fetch(`${apiBaseUrl}/api/Plc/connect`, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
        },
        body: JSON.stringify(request),
    })
}