import type { PlcAuthObject } from '../models/PlcAuthObject.ts'

export async function connectToPlc(request: PlcAuthObject): Promise<Response> {
    return fetch('https://localhost:61411/api/Plc/connect', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
        },
        body: JSON.stringify(request),
    })
}