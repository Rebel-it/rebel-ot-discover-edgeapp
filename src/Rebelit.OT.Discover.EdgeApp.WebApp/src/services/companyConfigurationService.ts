import type { CompanyConfiguration } from '../models/CompanyConfiguration'
import type { ServiceAccountObject } from '../models/ServiceAccountObject';
import { apiBaseUrl } from './apiBaseUrl'

export async function getCompanyConfiguration(serviceAccount: ServiceAccountObject): Promise<CompanyConfiguration> {

   const response = await fetch(`${apiBaseUrl}/CompanyConfiguration`, {
    method: 'GET',
    headers: {
      'Content-Type': 'application/json',
      'Api-application': serviceAccount.apiApplicationID,
      'Api-Access-Token': serviceAccount.accessToken
    }
  });

  if (!response.ok) {
    throw new Error(await response.text());
  }

  const data: CompanyConfiguration = await response.json();
  return data;
}