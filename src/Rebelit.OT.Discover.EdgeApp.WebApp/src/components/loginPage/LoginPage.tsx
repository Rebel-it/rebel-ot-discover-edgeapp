import { type ComponentProps, useState } from 'react'
import type { AuthObject } from '../../models/AuthObject.ts'
import { login } from '../../services/authenticationService.ts'
import { saveAuthToSession, loadAuthFromSession } from '../../services/sessionStorageService.ts'
import styles from './LoginPage.module.css'
import { useNavigate } from 'react-router-dom'
import FormField from '../shared/FormField'


type LoginFormSubmitEvent = Parameters<NonNullable<ComponentProps<'form'>['onSubmit']>>[0]

const defaultAuthObject: AuthObject = {
  APIapplicationID: '',
  AccessToken: ''
}

function LoginPage() {
  const [authObject, setAuthObject] = useState<AuthObject>(() => loadAuthFromSession() ?? defaultAuthObject)
  const [isSubmitting, setIsSubmitting] = useState(false)
  const [loginSucceeded, setLoginSucceeded] = useState(false)
  const [errorMessage, setErrorMessage] = useState('')
  const navigate = useNavigate()

  function setAuthProperty<K extends keyof AuthObject>(property: K, value: AuthObject[K]) {
    setAuthObject((currentAuthObject) => ({
      ...currentAuthObject,
      [property]: value,
    }))
  }

  async function handleSubmit(event: LoginFormSubmitEvent) {
    event.preventDefault()
    setIsSubmitting(true)
    setErrorMessage('')
    setLoginSucceeded(false)

    try {
      const response = await login(authObject)

      if (response.status === 200) {
        saveAuthToSession(authObject)
        setLoginSucceeded(true)
        return
      }

      let nextErrorMessage = 'Login failed. Please check your credentials and try again.'
      const responseText = await response.text()

      if (responseText) {
        nextErrorMessage = responseText
      }

      setErrorMessage(nextErrorMessage)
    } catch {
      setErrorMessage('Unable to reach the login service. Check that the API is running.')
    } finally {
      setIsSubmitting(false)
    }
  }

  return (
    <div className={styles.loginWrapper}>
      <form className={styles.loginForm} onSubmit={handleSubmit} noValidate>
        <h1>Sign in</h1>

        <FormField
          id="applicationid"
          label="API Application ID"
          value={authObject.APIapplicationID}
          onChange={(value) => setAuthProperty('APIapplicationID', value)}
          required
        />

        <FormField
          id="accesstoken"
          label="Access Token"
          type="password"
          value={authObject.AccessToken}
          onChange={(value) => setAuthProperty('AccessToken', value)}
          required
        />

        {errorMessage && <p className={`${styles.formMessage} ${styles.errorMessage}`}>{errorMessage}</p>}

        {loginSucceeded && (
          <p className={`${styles.formMessage} ${styles.successMessage}`}>
            Login succeeded. Continue to the next step.
          </p>
        )}

        <button type="submit" className={styles.loginButton} disabled={isSubmitting || loginSucceeded}>
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

export default LoginPage
