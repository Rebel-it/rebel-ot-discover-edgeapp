import { apiBaseUrl } from './apiBaseUrl'

export async function synchronizeVariables(): Promise<Response> {
       return fetch(`${apiBaseUrl}/api/scraper/variables`, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
           }
    })
}