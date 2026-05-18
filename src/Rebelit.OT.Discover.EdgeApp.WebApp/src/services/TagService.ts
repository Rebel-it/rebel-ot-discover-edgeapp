import { httpClient } from './httpClient'
import type { Tag } from '../models/Tag'
export type { Tag } from '../models/Tag'


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

export function createTag(request: CreateTagRequest): Promise<void> {
    return httpClient.post('/tags', request)
}

export function updateTag(identifier: string, request: CreateTagRequest): Promise<void> {
    return httpClient.put(`/tags/${identifier}`, request)
}

export function getTags(): Promise<Tag[]> {
    return httpClient.get('/tags')
}   

export function getFilledTags(): Promise<Tag[]> {
    return httpClient.get('/tags/prefilled')
}