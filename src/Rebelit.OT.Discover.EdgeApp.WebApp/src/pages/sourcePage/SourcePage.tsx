import { useNavigate } from 'react-router-dom'
import { useState, type ComponentProps } from 'react'
import Loginstyles from '../loginPage/LoginPage.module.css'
import type { SourceObject } from '../../models/SourceObject'
import { createSource } from '../../services/DataSourceService'
import FormField from '../shared/FormField'

type SourceFormSubmitEvent = Parameters<NonNullable<ComponentProps<'form'>['onSubmit']>>[0]

const defaultSourceObject: SourceObject = {
    DataSourceName: '',
    AgentId: '',
}

function SourcePage() {

    const navigate = useNavigate()
    const [sourceObject, setSourceObject] = useState<SourceObject>(defaultSourceObject)
    const [isSubmitting, setIsSubmitting] = useState(false)
    const [sourceCreationSucceeded, setSourceCreationSucceeded] = useState(false)
    const [errorMessage, setErrorMessage] = useState('')

    function setSourceProperty<K extends keyof SourceObject>(property: K, value: SourceObject[K]) {
        setSourceObject((currentSourceObject) => ({
            ...currentSourceObject,
            [property]: value,
        }))
    }

    async function handleSubmit(event: SourceFormSubmitEvent) {
        event.preventDefault()
        if (isSubmitting) return
        setIsSubmitting(true)
        setErrorMessage('')
        setSourceCreationSucceeded(false)
        try {
            await createSource(sourceObject)
            setSourceCreationSucceeded(true)
        } catch (error) {
            setErrorMessage(error instanceof Error ? error.message : 'Source creation failed. Please check your input and try again.')
        } finally {
            setIsSubmitting(false)
        }
    }

    return (
        <div className={Loginstyles.loginWrapper}>
            <form className={Loginstyles.loginForm} onSubmit={handleSubmit} noValidate>
                <h1>Create data source</h1>
                <FormField
                    id="sourceName"
                    label="Source name"
                    value={sourceObject.DataSourceName}
                    onChange={(value) => setSourceProperty('DataSourceName', value)}
                />

                <FormField
                    id="agentId"
                    label="Agent ID"
                    value={sourceObject.AgentId}
                    onChange={(value) => setSourceProperty('AgentId', value)}
                />

                {errorMessage && <p className={`${Loginstyles.formMessage} ${Loginstyles.errorMessage}`}>{errorMessage}</p>}

                {sourceCreationSucceeded && (
                    <p className={`${Loginstyles.formMessage} ${Loginstyles.successMessage}`}>
                        Source creation succeeded. Continue to the next step.
                    </p>
                )}

                <button type="submit" className={Loginstyles.loginButton} disabled={isSubmitting || sourceCreationSucceeded}>
                    {isSubmitting ? 'Creating...' : 'Create'}
                </button>
            </form>
            {sourceCreationSucceeded && (
                <button type="button" className={Loginstyles.nextButton} onClick={() => navigate('/variables')}>
                    Next
                </button>
            )}
        </div>
    )
}

export default SourcePage