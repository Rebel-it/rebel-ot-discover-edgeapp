import { useState } from 'react'
import styles from './DeviceConfigPage.module.css'
import sharedStyles from '../loginPage/LoginPage.module.css'
import { PushDeviceConfiguration } from '../../services/IxonSettingService'
import { loadIxonAuthenticationHeaders } from '../../services/sessionStorageService'

function DeviceConfigPage() {
    const [loading, setLoading] = useState(false)
    const [status, setStatus] = useState<{ type: 'success' | 'error'; message: string } | null>(null)

    async function handlePushConfiguration() {
        const auth = loadIxonAuthenticationHeaders()
        if (!auth?.AgentId) {
            setStatus({ type: 'error', message: 'No agent ID found. Please log in again.' })
            return
        }

        setLoading(true)
        setStatus(null)

        try {
            await PushDeviceConfiguration(auth.AgentId)
            setStatus({ type: 'success', message: 'Configuration pushed successfully.' })
        } catch {
            setStatus({ type: 'error', message: 'Failed to push configuration. Please try again.' })
        } finally {
            setLoading(false)
        }
    }

    return (
        <div className={sharedStyles.wrapper}>
            <div className={styles.card}>
                <h1>Device Configuration</h1>
                <p>
                    Push the latest configuration to your edge devices to ensure they are up to date with the latest settings and ready for discovery workflows.
                </p>
                {status && (
                    <p className={`${styles.formMessage} ${status.type === 'success' ? styles.successMessage : styles.errorMessage}`}>
                        {status.message}
                    </p>
                )}
                <button
                    type="button"
                    className={sharedStyles.loginButton}
                    onClick={handlePushConfiguration}
                    disabled={loading}
                >
                    {loading ? 'Pushing...' : 'Push Configuration'}
                </button>
            </div>
        </div>
    )
}

export default DeviceConfigPage