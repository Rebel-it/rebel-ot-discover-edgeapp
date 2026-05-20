import { httpClient } from './httpClient'

export function PushDeviceConfiguration(agentId: string): Promise<string> {
    return httpClient.post<string>('/IxonSettings/pushConfiguration', { agentId })
}