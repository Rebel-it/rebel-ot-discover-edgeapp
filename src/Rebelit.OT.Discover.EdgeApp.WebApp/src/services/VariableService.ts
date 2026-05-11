import { httpClient } from './httpClient'

export type VariableOption = {
    label: string
    publicId: string
}

function getFirstString(candidate: Record<string, unknown>, keys: string[]): string | null {
    for (const key of keys) {
        const value = candidate[key]
        if (typeof value === 'string' && value.trim()) {
            return value
        }
    }

    return null
}

function toVariableOption(value: unknown): VariableOption | null {
    if (typeof value === 'string' && value.trim()) {
        return { label: value, publicId: value }
    }

    if (value && typeof value === 'object') {
        const candidate = value as Record<string, unknown>
        const label = getFirstString(candidate, [
            'name',
            'Name',
            'variableName',
            'VariableName',
            'identifier',
            'Identifier',
            'slug',
            'Slug',
            'publicId',
            'PublicId',
            'id',
            'Id',
        ])
        const publicId = getFirstString(candidate, ['publicId', 'PublicId', 'id', 'Id', 'identifier', 'Identifier'])

        if (label && publicId) {
            return { label, publicId }
        }
    }

    return null
}


export async function getVariables(): Promise<VariableOption[]> {
    let payload: unknown

    try {
        payload = await httpClient.get<unknown>('/variable')
    } catch {
        return []
    }

    if (!Array.isArray(payload)) return []

    return payload.map(toVariableOption).filter((value): value is VariableOption => Boolean(value))
}