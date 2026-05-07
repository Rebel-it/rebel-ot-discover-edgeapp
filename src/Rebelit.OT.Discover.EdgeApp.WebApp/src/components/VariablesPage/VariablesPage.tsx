import { useEffect, useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { synchronizeVariables } from '../../services/ScraperService'
import sharedStyles from '../loginPage/LoginPage.module.css'
import styles from './VariablesPage.module.css'

function VariablesPage() {
    const navigate = useNavigate()
    const [isSubmitting, setIsSubmitting] = useState(true)
    const [synchronizationSucceeded, setSynchronizationSucceeded] = useState(false)
    const [errorMessage, setErrorMessage] = useState('')

    useEffect(() => {
        let isActive = true

        async function runSynchronization() {
            setIsSubmitting(true)
            setErrorMessage('')
            setSynchronizationSucceeded(false)

            try {
                await synchronizeVariables()

                if (!isActive) return

                setSynchronizationSucceeded(true)
            } catch (error) {
                if (!isActive) return

                setErrorMessage(error instanceof Error ? error.message : 'Variable synchronization failed. Please try again.')
            } finally {
                if (isActive) {
                    setIsSubmitting(false)
                }
            }
        }

        void runSynchronization()

        return () => {
            isActive = false
        }
    }, [])

    return (
        <div className={sharedStyles.loginWrapper}>
            <div className={sharedStyles.loginForm}>
                <h1>Synchronize variables</h1>

                {isSubmitting && (
                    <div className={styles.statusBlock}>
                        <div className={styles.spinner} aria-hidden="true" />
                        <p className={styles.statusText}>Variable synchronization is in progress. This may take a few moments.</p>
                    </div>
                )}

                {errorMessage && <p className={`${sharedStyles.formMessage} ${sharedStyles.errorMessage}`}>{errorMessage}</p>}

                {synchronizationSucceeded && (
                    <p className={`${sharedStyles.formMessage} ${sharedStyles.successMessage}`}>
                        Variable synchronization succeeded. Continue to the next step.
                    </p>
                )}
            </div>

            {synchronizationSucceeded && (
                <button type="button" className={sharedStyles.nextButton} onClick={() => navigate('/')}>
                    Next
                </button>
            )}
        </div>
    )
}

export default VariablesPage