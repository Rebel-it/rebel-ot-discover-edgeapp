import { type ComponentProps, useState } from 'react'
import type { AuthObject } from '../../models/AuthObject.ts'
import { login } from '../../services/authenticationService.ts'
import styles from './LoginPage.module.css'
import { useNavigate } from 'react-router-dom'
import FormField from '../shared/FormField'


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
          id="username"
          label="Username"
          value={authObject.username}
          onChange={(value) => setAuthProperty('username', value)}
          required
        />

        <FormField
          id="password"
          label="Password"
          type="password"
          value={authObject.password}
          onChange={(value) => setAuthProperty('password', value)}
          required
        />
        <FormField 
        id="application-id"
          label="Application ID"
          value={authObject.applicationID}
          onChange={(value) => setAuthProperty('applicationID', value)}
          required
        />

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

            <FormField
              id="otp"
              label="OTP code"
              autoComplete="one-time-code"
              value={authObject.otpCode ?? ''}
              onChange={(value) => setAuthProperty('otpCode', value.replaceAll(/\D/gu, ''))}
              required
            />
          </div>
        )}

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
