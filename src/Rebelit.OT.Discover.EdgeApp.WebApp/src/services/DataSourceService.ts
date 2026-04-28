import type { SourceObject } from '../models/SourceObject'
import { apiBaseUrl } from './apiBaseUrl'
export async function createSource(request: SourceObject): Promise<Response> {
    return fetch(`${apiBaseUrl}/api/IxonSettings/datasource`, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
        },
        body: JSON.stringify(request),
    })
}