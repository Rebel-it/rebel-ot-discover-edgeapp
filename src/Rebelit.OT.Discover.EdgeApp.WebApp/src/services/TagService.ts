import { apiBaseUrl } from './apiBaseUrl'

export type CreateTagRequest = {
    logEvent: 'interval' | 'change' | 'trigger'
    loggingInterval: string
    name: string
    onChangeExpiry: string | null
    retentionPolicy: string
    slug: string
    variable: string
    
    edgeAggregator: string | null
}

export type Tag = {
    variable: string
    name: string
    identifier: string
}

export async function createTag(request: CreateTagRequest): Promise<Response> {
    return fetch(`${apiBaseUrl}/api/tags`, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
        },
        body: JSON.stringify(request),
    })
}
