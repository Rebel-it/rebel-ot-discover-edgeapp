import type { CompanyConfiguration } from '../models/CompanyConfiguration'
import { httpClient } from './httpClient'

export function getCompanyConfiguration(): Promise<CompanyConfiguration> {
  return httpClient.get<CompanyConfiguration>('/CompanyConfiguration')
}