import { useNavigate } from 'react-router-dom'
import { savePlcAuth } from '../../services/sessionStorageService.ts'
import { connectToPlc } from '../../services/plcService.ts'
import type { PlcAuthObject } from '../../models/PlcAuthObject'
import { useState, type ComponentProps } from 'react'
import FormField from '../atoms/formField/FormField.tsx'
import styles from './PlcConnect.module.css'

type PlcFormSubmitEvent = Parameters<NonNullable<ComponentProps<'form'>['onSubmit']>>[0]

const defaultPlcObject: PlcAuthObject = {
    OpcUaServerAddress: '',
    OpcUaUsername: '',
    OpcUaPassword: '',
}

function PlcConnect() {
    const navigate = useNavigate()
    const [plcObject, setPlcObject] = useState<PlcAuthObject>(defaultPlcObject)
    const [useCredentials, setUseCredentials] = useState(false)
    const [isSubmitting, setIsSubmitting] = useState(false)
    const [connectionSucceeded, setConnectionSucceeded] = useState(false)
    const [errorMessage, setErrorMessage] = useState('')

    function setPlcProperty<K extends keyof PlcAuthObject>(property: K, value: PlcAuthObject[K]) {
        setPlcObject((currentPlcObject) => ({
            ...currentPlcObject,
            [property]: value,
        }))
    }

    function handleUseCredentialsChange(checked: boolean) {
        setUseCredentials(checked)

        if (!checked) {
            setPlcObject((currentPlcObject) => ({
                ...currentPlcObject,
                OpcUaUsername: '',
                OpcUaPassword: '',
            }))
        }
    }

    async function handleSubmit(event: PlcFormSubmitEvent) {
        event.preventDefault()
        setIsSubmitting(true)
        setErrorMessage('')
        setConnectionSucceeded(false)

        try {
            await connectToPlc(plcObject)
            savePlcAuth(plcObject)
            setConnectionSucceeded(true)
        } catch (error) {
            setErrorMessage(error instanceof Error ? error.message : 'PLC connection failed. Please check your credentials and try again.')
        } finally {
            setIsSubmitting(false)
        }
    }

    return (
        <div className={styles.loginWrapper}>
            <form className={styles.loginForm} onSubmit={handleSubmit} noValidate>
                <h1>PLC Connect</h1>

                <FormField
                    id="ipAddress"
                    label="PLC Server IP Address"
                    value={plcObject.OpcUaServerAddress}
                    onChange={(value) => setPlcProperty('OpcUaServerAddress', value)}
                    required
                />

                <label>
                    <input
                        type="checkbox"
                        checked={useCredentials}
                        onChange={(event) => handleUseCredentialsChange(event.target.checked)}
                    />
                    Use PLC username and password
                </label>

                {useCredentials && (
                    <>
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
                    </>
                )}

                {errorMessage && <p className={`${styles.formMessage} ${styles.errorMessage}`}>{errorMessage}</p>}

                {connectionSucceeded && (
                    <p className={`${styles.formMessage} ${styles.successMessage}`}>
                        PLC connection succeeded. Continue to the next step.
                    </p>
                )}

                <button type="submit" className={styles.loginButton} disabled={isSubmitting || connectionSucceeded}>
                    {isSubmitting ? 'Connecting...' : 'Connect'}
                </button>
            </form>

            {connectionSucceeded && (
                <button type="button" className={styles.nextButton} onClick={() => navigate('/source')}>
                    Next
                </button>
            )}
        </div>
    )
}

export default PlcConnect