import type { CompanyConfiguration } from '../models/CompanyConfiguration'
import type { IxonAuthenticationHeaders } from '../models/IxonAuthenticationHeaders'
import type { ServiceAccountObject } from '../models/ServiceAccountObject'
import type { PlcAuthObject } from '../models/PlcAuthObject';

const API_APPLICATION_ID_KEY = 'apiApplicationId';
const ACCESS_TOKEN_KEY = 'accessToken';
const COMPANY_ID = 'companyId';
const AGENT_ID = 'agentId';
const PLC_URL = 'OpcUaServerAddress';
const PLC_USERNAME = 'OpcUaUsername';
const PLC_PASSWORD = 'OpcUaPassword';

export function saveIxonAuth(auth: ServiceAccountObject, config: CompanyConfiguration): void {
  sessionStorage.setItem(API_APPLICATION_ID_KEY, auth.apiApplicationID);
  sessionStorage.setItem(ACCESS_TOKEN_KEY, auth.accessToken);
  sessionStorage.setItem(COMPANY_ID, config.companyId ?? '');
  sessionStorage.setItem(AGENT_ID, config.agentId ?? '');
}

export function savePlcAuth(plcAuth: PlcAuthObject): void {
  sessionStorage.setItem(PLC_URL, plcAuth.OpcUaServerAddress);
  sessionStorage.setItem(PLC_USERNAME, plcAuth.OpcUaUsername);
  sessionStorage.setItem(PLC_PASSWORD, plcAuth.OpcUaPassword);
}

export function loadIxonAuthenticationHeaders(): IxonAuthenticationHeaders | null {
  const apiApplicationId = sessionStorage.getItem(API_APPLICATION_ID_KEY);
  const accessToken = sessionStorage.getItem(ACCESS_TOKEN_KEY);
  const companyId = sessionStorage.getItem(COMPANY_ID);
  const agentId = sessionStorage.getItem(AGENT_ID);
  const opcUaServerAddress = sessionStorage.getItem(PLC_URL);
  const opcUaUsername = sessionStorage.getItem(PLC_USERNAME);
  const opcUaPassword = sessionStorage.getItem(PLC_PASSWORD);
 
  if (!apiApplicationId || !accessToken || !companyId || !agentId|| !opcUaServerAddress || !opcUaUsername || !opcUaPassword) {
    return null;
  }

  return {
    ApplicationId: apiApplicationId,
    AccessToken: accessToken,
    CompanyId: companyId,
    AgentId: agentId,
    OpcUaServerAddress: opcUaServerAddress,
    OpcUaUsername: opcUaUsername,
    OpcUaPassword: opcUaPassword  
  }
}

export function clearIxonAuthenticationHeaders(): void {
  sessionStorage.removeItem(API_APPLICATION_ID_KEY);
  sessionStorage.removeItem(ACCESS_TOKEN_KEY);
  sessionStorage.removeItem(COMPANY_ID);
  sessionStorage.removeItem(AGENT_ID);
  sessionStorage.removeItem(PLC_URL);
  sessionStorage.removeItem(PLC_USERNAME);
  sessionStorage.removeItem(PLC_PASSWORD);
}
