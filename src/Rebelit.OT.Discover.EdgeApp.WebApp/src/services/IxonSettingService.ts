import { httpClient } from './httpClient'

export function PushDeviceConfiguration(agentId: string): Promise<string> {
    return httpClient.post('/IxonSettings/pushConfiguration', { agentId })
}