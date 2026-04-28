import { useNavigate } from 'react-router-dom'
import { connectToPlc } from '../../services/PlcService.ts'
import Loginstyles from '../loginPage/LoginPage.module.css'
import type { PlcAuthObject } from '../../models/PlcAuthObject'
import { useState, type ComponentProps } from 'react'

type PlcFormSubmitEvent = Parameters<NonNullable<ComponentProps<'form'>['onSubmit']>>[0]

const defaultPlcObject: PlcAuthObject = {
    OpcUaServerAddress: '',
    OpcUaUsername: '',
    OpcUaPassword: '',
}

function PlcConnect() {
    const navigate = useNavigate()
    const [plcObject, setPlcObject] = useState<PlcAuthObject>(defaultPlcObject)
    const [isSubmitting, setIsSubmitting] = useState(false)
    const [connectionSucceeded, setConnectionSucceeded] = useState(false)
    const [errorMessage, setErrorMessage] = useState('')

    function setPlcProperty<K extends keyof PlcAuthObject>(property: K, value: PlcAuthObject[K]) {
        setPlcObject((currentPlcObject) => ({
            ...currentPlcObject,
            [property]: value,
        }))
    }

    async function handleSubmit(event: PlcFormSubmitEvent) {
        event.preventDefault()
        setIsSubmitting(true)
        setErrorMessage('')
        setConnectionSucceeded(false)

        try {
            const response = await connectToPlc(plcObject)

            if (response.status === 200) {
                setConnectionSucceeded(true)
                return
            }

            let nextErrorMessage = 'PLC connection failed. Please check your credentials and try again.'
      const responseText = await response.text()

      if (responseText) {
        nextErrorMessage = responseText
      }

      setErrorMessage(nextErrorMessage)
    } catch {
            setErrorMessage('Unable to reach the PLC service. Check that the API is running.')
    } finally {
      setIsSubmitting(false)
    }
    }

    return (
        <div className={Loginstyles.loginWrapper}>
            <form className={Loginstyles.loginForm} onSubmit={handleSubmit} noValidate>
                <h1>PLC Connect</h1>

                <div className={Loginstyles.formField}>
                    <label htmlFor="ipAddress">PLC Server IP Address</label>
                    <input
                        id="ipAddress"
                        type="text"
                        autoComplete="off"
                        value={plcObject.OpcUaServerAddress}
                        onChange={(e) => setPlcProperty('OpcUaServerAddress', e.target.value)}
                        required
                    />
                </div>

                <div className={Loginstyles.formField}>
                    <label htmlFor="OpcUaUsername">PLC User Name</label>
                    <input
                        id="OpcUaUsername"
                        type="text"
                        autoComplete="off"
                        value={plcObject.OpcUaUsername}
                        onChange={(e) => setPlcProperty('OpcUaUsername', e.target.value)}
                    />
                </div>

                <div className={Loginstyles.formField}>
                    <label htmlFor="OpcUaPassword">PLC Password</label>
                    <input
                        id="OpcUaPassword"
                        type="password"
                        autoComplete="off"
                        value={plcObject.OpcUaPassword}
                        onChange={(e) => setPlcProperty('OpcUaPassword', e.target.value)}
                    />
                </div>

                {errorMessage && <p className={`${Loginstyles.formMessage} ${Loginstyles.errorMessage}`}>{errorMessage}</p>}

                {connectionSucceeded && (
                    <p className={`${Loginstyles.formMessage} ${Loginstyles.successMessage}`}>
                        PLC connection succeeded. Continue to the next step.
                    </p>
                )}

                <button type="submit" className={Loginstyles.loginButton} disabled={isSubmitting || connectionSucceeded}>
                    {isSubmitting ? 'Connecting...' : 'Connect'}
                </button>
            </form>

            {connectionSucceeded && (
                <button type="button" className={Loginstyles.nextButton} onClick={() => navigate('/plc')}>
                    Next
                </button>
            )}
        </div>
    )
}

export default PlcConnect