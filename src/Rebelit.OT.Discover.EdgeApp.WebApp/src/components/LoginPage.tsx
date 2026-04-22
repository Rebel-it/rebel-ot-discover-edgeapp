import { useState } from 'react'
import './LoginPage.css'

function LoginPage() {
  const [username, setUsername] = useState('')
  const [password, setPassword] = useState('')
  const [otpEnabled, setOtpEnabled] = useState(false)
  const [otp, setOtp] = useState('')

  return (
    <div className="login-wrapper">
      <form
        className="login-form"
        onSubmit={(e) => {
          e.preventDefault()
          // Submit credentials: username, password, and optionally otp
        }}
        noValidate
      >
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
              autoComplete="one-time-code"
              value={otp}
              onChange={(e) => setOtp(e.target.value.replaceAll(/\D/gu, ''))}
              required={otpEnabled}
            />
          </div>
        )}

        <button type="submit" className="login-btn">
          Sign in
        </button>
      </form>
    </div>
  )
}

export default LoginPage
