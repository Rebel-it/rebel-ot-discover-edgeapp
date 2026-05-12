import { useNavigate } from 'react-router-dom'
import { connectToPlc } from '../../services/PlcService.ts'
import Loginstyles from '../loginPage/LoginPage.module.css'
import type { PlcAuthObject } from '../../models/PlcAuthObject'
import { useState, type ComponentProps } from 'react'
import FormField from '../Atoms/FormField/FormField.tsx'

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
            await connectToPlc(plcObject)
            setConnectionSucceeded(true)
        } catch (error) {
            setErrorMessage(error instanceof Error ? error.message : 'PLC connection failed. Please check your credentials and try again.')
        } finally {
            setIsSubmitting(false)
        }
    }

    return (
        <div className={Loginstyles.loginWrapper}>
            <form className={Loginstyles.loginForm} onSubmit={handleSubmit} noValidate>
                <h1>PLC Connect</h1>

                <FormField
                    id="ipAddress"
                    label="PLC Server IP Address"
                    value={plcObject.OpcUaServerAddress}
                    onChange={(value) => setPlcProperty('OpcUaServerAddress', value)}
                    required
                />

                <FormField
                    id="OpcUaUsername"
                    label="PLC User Name"
                    value={plcObject.OpcUaUsername}
                    onChange={(value) => setPlcProperty('OpcUaUsername', value)}
                />

                <FormField
                    id="OpcUaPassword"
                    label="PLC Password"
                    type="password"
                    value={plcObject.OpcUaPassword}
                    onChange={(value) => setPlcProperty('OpcUaPassword', value)}
                />

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
                <button type="button" className={Loginstyles.nextButton} onClick={() => navigate('/source')}>
                    Next
                </button>
            )}
        </div>
    )
}

export default PlcConnect