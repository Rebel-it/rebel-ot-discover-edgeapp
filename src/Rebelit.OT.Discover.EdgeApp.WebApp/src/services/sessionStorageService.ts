import type { CompanyConfiguration } from '../models/CompanyConfiguration'
import type { IxonAuthenticationHeaders } from '../models/IxonAuthenticationHeaders'
import type { ServiceAccountObject } from '../models/ServiceAccountObject'

const API_APPLICATION_ID_KEY = 'apiApplicationId';
const ACCESS_TOKEN_KEY = 'accessToken';
const COMPANY_ID = 'companyId';
const AGENT_ID = 'agentId';

export function SaveIxonAuthenticationHeaders(auth: ServiceAccountObject, config: CompanyConfiguration | null): void {
  sessionStorage.setItem(COMPANY_ID, config?.companyId ?? '');
  sessionStorage.setItem(AGENT_ID, config?.agentId ?? '');
  sessionStorage.setItem(API_APPLICATION_ID_KEY, auth.apiApplicationID);
  sessionStorage.setItem(ACCESS_TOKEN_KEY, auth.accessToken);
}

export function loadIxonAuthenticationHeaders(): IxonAuthenticationHeaders | null {
  const apiApplicationId = sessionStorage.getItem(API_APPLICATION_ID_KEY);
  const accessToken = sessionStorage.getItem(ACCESS_TOKEN_KEY);
  const companyId = sessionStorage.getItem(COMPANY_ID);
  const agentId = sessionStorage.getItem(AGENT_ID);
 
  if (!apiApplicationId || !accessToken || !companyId || !agentId) {
    return null;
  }

  return {
    ApplicationId: apiApplicationId,
    AccessToken: accessToken,
    CompanyId: companyId,
    AgentId: agentId
  }
}

export function clearIxonAuthenticationHeaders(): void {
  sessionStorage.removeItem(API_APPLICATION_ID_KEY);
  sessionStorage.removeItem(ACCESS_TOKEN_KEY);
  sessionStorage.removeItem(COMPANY_ID);
  sessionStorage.removeItem(AGENT_ID);
}
