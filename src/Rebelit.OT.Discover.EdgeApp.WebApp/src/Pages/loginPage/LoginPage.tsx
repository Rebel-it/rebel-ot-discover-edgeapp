import { useState, type ComponentProps } from 'react'
import { saveIxonAuth, clearIxonAuthenticationHeaders } from '../../services/sessionStorageService.ts'
import styles from './LoginPage.module.css'
import { useNavigate } from 'react-router-dom'
import { getCompanyConfiguration } from '../../services/companyConfigurationService.ts'
import type { ServiceAccountObject } from '../../models/ServiceAccountObject.ts'
import FormField from '../../components/atoms/formField/FormField.tsx'

type LoginFormSubmitEvent = Parameters<NonNullable<ComponentProps<'form'>['onSubmit']>>[0]

const defaultAuthObject: ServiceAccountObject = {
  apiApplicationID: '',
  accessToken: ''
}

function LoginPage() {
  const [serviceAccount, setServiceAccount] = useState<ServiceAccountObject>(defaultAuthObject);
  const [loginSucceeded, setLoginSucceeded] = useState(false);
  const [errorMessage, setErrorMessage] = useState<string>("");
  const [isSubmitting, setIsSubmitting] = useState(false);
  const navigate = useNavigate();

  function setAuthProperty<K extends keyof ServiceAccountObject>(property: K, value: ServiceAccountObject[K]) {
    setServiceAccount((currentServiceAccount) => ({
      ...currentServiceAccount,
      [property]: value,
    }));
  }

  async function handleSubmit(event: LoginFormSubmitEvent) {
    event.preventDefault();

    try {
      setIsSubmitting(true);
      const result = await getCompanyConfiguration(serviceAccount);
      saveIxonAuth(serviceAccount, result);
      setLoginSucceeded(true);
      setErrorMessage("");
    } catch (error) {
      clearIxonAuthenticationHeaders();
      setErrorMessage(error instanceof Error ? error.message : 'Failed to login. Please check your credentials and try again.');
    } finally {
      setIsSubmitting(false);
    }
  }

  return (
    <div className={styles.loginWrapper}>
      <form className={styles.loginForm} onSubmit={handleSubmit} noValidate>
        <h1>Sign in</h1>

        <FormField
          id="applicationid"
          label="API Application ID"
          value={serviceAccount.apiApplicationID}
          onChange={(value) => setAuthProperty('apiApplicationID', value)}
          required
        />

        <FormField
          id="accesstoken"
          label="Access Token"
          type="password"
          value={serviceAccount.accessToken}
          onChange={(value) => setAuthProperty('accessToken', value)}
          required
        />
        
        {errorMessage && <p className={`${styles.formMessage} ${styles.errorMessage}`}>{errorMessage}</p>}


        {loginSucceeded && (
          <p className={`${styles.formMessage} ${styles.successMessage}`}>
            Login succeeded. Continue to the next step.
          </p>
        )}

        <button type="submit" className={styles.loginButton} disabled={isSubmitting}>
          {isSubmitting ? 'Signing in...' : 'Sign in'}
        </button>
      </form>

      {loginSucceeded && (
        <button type="button" className={styles.nextButton} onClick={() => navigate('/plc')}>
          Next
        </button>
      )}
    </div>
  )
}

export default LoginPage;