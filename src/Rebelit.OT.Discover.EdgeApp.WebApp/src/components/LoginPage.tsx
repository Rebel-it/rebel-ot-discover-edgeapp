import { type ComponentProps, useState } from 'react'
import { login } from '../services/authenticationService.ts'
import './LoginPage.css'

const defaultApplicationId = 'rebelit-ot-discover-edgeapp-webapp'
type LoginFormSubmitEvent = Parameters<NonNullable<ComponentProps<'form'>['onSubmit']>>[0]

function LoginPage() {
  const [username, setUsername] = useState('')
  const [password, setPassword] = useState('')
  const [otpEnabled, setOtpEnabled] = useState(false)
  const [otp, setOtp] = useState('')
  const [applicationId, setApplicationId] = useState(defaultApplicationId)
  const [isSubmitting, setIsSubmitting] = useState(false)
  const [loginSucceeded, setLoginSucceeded] = useState(false)
  const [errorMessage, setErrorMessage] = useState('')

  async function handleSubmit(event: LoginFormSubmitEvent) {
    event.preventDefault()
    setIsSubmitting(true)
    setErrorMessage('')
    setLoginSucceeded(false)

    try {
      const response = await login({
        username,
        password,
        otpCode: otpEnabled ? otp : '',
        applicationID: applicationId,
      })

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
    <div className="login-wrapper">
      <form className="login-form" onSubmit={handleSubmit} noValidate>
        <h1>Sign in</h1>

        <div className="form-field">
          <label htmlFor="username">Username</label>
          <input
            id="username"
            type="text"
            autoComplete="username"
            value={username}
            onChange={(e) => setUsername(e.target.value)}
            required
          />
        </div>

        <div className="form-field">
          <label htmlFor="password">Password</label>
          <input
            id="password"
            type="password"
            autoComplete="current-password"
            value={password}
            onChange={(e) => setPassword(e.target.value)}
            required
          />
        </div>

        <div className="form-field">
          <label htmlFor="application-id">Application ID</label>
          <input
            id="application-id"
            type="text"
            value={applicationId}
            onChange={(e) => setApplicationId(e.target.value)}
            required
          />
        </div>

        <label className="checkbox-label">
          <input
            type="checkbox"
            checked={otpEnabled}
            onChange={(e) => {
              setOtpEnabled(e.target.checked)
              if (!e.target.checked) setOtp('')
            }}
          />{' '}
          Use one-time password (OTP)
        </label>

        {otpEnabled && (
          <div className="form-field">
            <label htmlFor="otp">OTP code</label>
            <input
              id="otp"
              type="text"
              inputMode="numeric"
              autoComplete="one-time-code"
              value={otp}
              onChange={(e) => setOtp(e.target.value.replaceAll(/\D/gu, ''))}
              required={otpEnabled}
            />
          </div>
        )}

        {errorMessage && <p className="form-message error-message">{errorMessage}</p>}

        {loginSucceeded && (
          <p className="form-message success-message">Login succeeded. Continue to the next step.</p>
        )}

        <button type="submit" className="login-btn" disabled={isSubmitting}>
          {isSubmitting ? 'Signing in...' : 'Sign in'}
        </button>
      </form>

      {loginSucceeded && (
        <button type="button" className="next-btn">
          Next
        </button>
      )}
    </div>
  )
}

export default LoginPage
