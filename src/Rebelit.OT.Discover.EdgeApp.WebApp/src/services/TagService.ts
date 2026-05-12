import { httpClient } from './httpClient'


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

export function createTag(request: CreateTagRequest): Promise<void> {
    return httpClient.post('/tags', request)
}

export function updateTag(identifier: string, request: CreateTagRequest): Promise<void> {
    return httpClient.put(`/tags/${identifier}`, request)
}
