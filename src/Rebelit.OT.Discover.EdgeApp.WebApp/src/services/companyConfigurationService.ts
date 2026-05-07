import type { ApiResponseObject } from '../models/ApiResponseObject';
import type { CompanyConfiguration } from '../models/CompanyConfiguration'
import type { ServiceAccountObject } from '../models/ServiceAccountObject';
import { apiBaseUrl } from './apiBaseUrl'

export async function getCompanyConfiguration(serviceAccount: ServiceAccountObject): Promise<ApiResponseObject<CompanyConfiguration>> {

   const response = await fetch(`${apiBaseUrl}/CompanyConfiguration`, {
    method: 'GET',
    headers: {
      'Content-Type': 'application/json',
      'Api-application': serviceAccount.apiApplicationID,
      'Api-Access-Token': serviceAccount.accessToken
    }
  });

  if (!response.ok) {
    throw new Error(`Error fetching company configuration: ${response.statusText}`);
  }

  const data: ApiResponseObject<CompanyConfiguration> = await response.json();
  return data;
}