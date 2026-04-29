import { apiBaseUrl } from './apiBaseUrl'

export async function VariableScraper(): Promise<Response> {
       return fetch(`${apiBaseUrl}/api/scraper/variables`, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
           }
    })
}