export type TagLogEvent = 'interval' | 'change' | 'trigger'

export type TagVariable = {
    publicId: string
}

export type Tag = {
    logEvent: TagLogEvent
    loggingInterval: string
    name: string
    onChangeExpiry: string | null
    retentionPolicy: string
    slug: string
    variable: TagVariable
    edgeAggregator: string | null
}