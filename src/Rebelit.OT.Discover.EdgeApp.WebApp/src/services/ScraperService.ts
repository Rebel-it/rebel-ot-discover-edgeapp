import { apiBaseUrl } from './apiBaseUrl'

export async function getVariables(): Promise<string[]> {
    const response = await fetch(`${apiBaseUrl}/api/scraper/variables`, {
        method: 'GET',
        headers: {
            'Content-Type': 'application/json',
        },
    })
    if (!response.ok) return []
    return response.json() as Promise<string[]>
}

export async function synchronizeVariables(): Promise<Response> {
       return fetch(`${apiBaseUrl}/api/scraper/variables`, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
           }
    })
}