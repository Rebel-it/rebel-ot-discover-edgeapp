import type { AuthObject } from '../models/AuthObject'
import type { CompanyConfiguration } from '../models/CompanyConfiguration'

const API_APPLICATION_ID_KEY = 'apiApplicationId'
const ACCESS_TOKEN_KEY = 'accessToken'
const COMPANY_ID = 'companyId'
const AGENT_ID = 'agentId'

export function saveAuthToSession(auth: AuthObject): void {
  sessionStorage.setItem(API_APPLICATION_ID_KEY, auth.APIapplicationID)
  sessionStorage.setItem(ACCESS_TOKEN_KEY, auth.AccessToken)
}

export function saveCompanyConfigurationToSession(config: CompanyConfiguration): void {
  sessionStorage.setItem(COMPANY_ID, config.CompanyId)
  sessionStorage.setItem(AGENT_ID, config.AgentId)
}

export function loadAuthFromSession(): AuthObject | null {
  const apiApplicationId = sessionStorage.getItem(API_APPLICATION_ID_KEY)
  const accessToken = sessionStorage.getItem(ACCESS_TOKEN_KEY)

  if (!apiApplicationId || !accessToken) {
    return null
  }

  return {
    APIapplicationID: apiApplicationId,
    AccessToken: accessToken,
  }
}

export function clearAuthFromSession(): void {
  sessionStorage.removeItem(API_APPLICATION_ID_KEY)
  sessionStorage.removeItem(ACCESS_TOKEN_KEY)
}

export function clearCompanyConfigurationFromSession(): void {
  sessionStorage.removeItem(COMPANY_ID)
  sessionStorage.removeItem(AGENT_ID)
}
