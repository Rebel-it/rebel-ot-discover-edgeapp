import type { AuthObject } from '../models/AuthObject'
import type { CompanyConfiguration } from '../models/CompanyConfiguration'
import { apiBaseUrl } from './apiBaseUrl'

export async function getCompanyConfiguration(request: AuthObject): Promise<CompanyConfiguration> {
  const response = await fetch(`${apiBaseUrl}/CompanyConfiguration`, {
    method: 'GET',
    headers: {
      'Content-Type': 'application/json',
      'Api-application': request.APIapplicationID,
      'Api-Access-Token': request.AccessToken
    }
  });

  if (!response.ok) {
    throw new Error(`Error fetching company configuration: ${response.statusText}`);
  }

  const data: CompanyConfiguration = await response.json();
  return data;
}