import { type ComponentProps, useState } from 'react'
import type { AuthObject } from '../models/AuthObject'
import { login } from '../services/authenticationService.ts'
import styles from './LoginPage.module.css'


type LoginFormSubmitEvent = Parameters<NonNullable<ComponentProps<'form'>['onSubmit']>>[0]

const defaultAuthObject: AuthObject = {
  username: '',
  password: '',
  otpCode: '',
  applicationID: '',
}

function LoginPage() {
  const [authObject, setAuthObject] = useState<AuthObject>(defaultAuthObject)
  const [otpEnabled, setOtpEnabled] = useState(false)
  const [isSubmitting, setIsSubmitting] = useState(false)
  const [loginSucceeded, setLoginSucceeded] = useState(false)
  const [errorMessage, setErrorMessage] = useState('')

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
        setLoginSucceeded(true)
        return
      }

      let nextErrorMessage = 'Login failed. Please check your credentials and try again.'

      try {
        const contentType = response.headers.get('content-type') ?? ''

        if (contentType.includes('application/json')) {
          const responseBody: unknown = await response.json()

          if (
            typeof responseBody === 'object' &&
            responseBody !== null &&
            'message' in responseBody &&
            typeof responseBody.message === 'string' &&
            responseBody.message.trim() !== ''
          ) {
            nextErrorMessage = responseBody.message
          }
        } else {
          const responseText = await response.text()

          if (responseText.trim() !== '') {
            nextErrorMessage = responseText
          }
        }
      } catch {
        // Fall back to the generic error message when the error payload cannot be parsed.
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

        <div className={styles.formField}>
          <label htmlFor="username">Username</label>
          <input
            id="username"
            type="text"
            autoComplete="username"
            value={authObject.username}
            onChange={(e) => setAuthProperty('username', e.target.value)}
            required
          />
        </div>

        <div className={styles.formField}>
          <label htmlFor="password">Password</label>
          <input
            id="password"
            type="password"
            autoComplete="current-password"
            value={authObject.password}
            onChange={(e) => setAuthProperty('password', e.target.value)}
            required
          />
        </div>

        <div className={styles.formField}>
          <label htmlFor="application-id">Application ID</label>
          <input
            id="application-id"
            type="text"
            value={authObject.applicationID}
            onChange={(e) => setAuthProperty('applicationID', e.target.value)}
            required
          />
        </div>

        <label className={styles.checkboxLabel}>
          <input
            type="checkbox"
            checked={otpEnabled}
            onChange={(e) => {
              setOtpEnabled(e.target.checked)
              if (!e.target.checked) setAuthProperty('otpCode', '')
            }}
          />{' '}
          Use one-time password (OTP)
        </label>

        {otpEnabled && (
          <div className={styles.formField}>
            <label htmlFor="otp">OTP code</label>
            <input
              id="otp"
              type="text"
              inputMode="numeric"
              autoComplete="one-time-code"
              value={authObject.otpCode}
              onChange={(e) => setAuthProperty('otpCode', e.target.value.replaceAll(/\D/gu, ''))}
              required={otpEnabled}
            />
          </div>
        )}

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
        <button type="button" className={styles.nextButton}>
          Next
        </button>
      )}
    </div>
  )
}

export default LoginPage
