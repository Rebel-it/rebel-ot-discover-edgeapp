import { useNavigate } from 'react-router-dom'
import { useState, type ComponentProps } from 'react'
import { loadPlcServerAddress, saveSourceId } from '../../services/sessionStorageService.ts'
import Loginstyles from '../loginPage/LoginPage.module.css'
import type { SourceObject } from '../../models/SourceObject.ts'
import { createSource } from '../../services/dataSourceService.ts'
import FormField from '../../components/atoms/formField/FormField.tsx'

type SourceFormSubmitEvent = Parameters<NonNullable<ComponentProps<'form'>['onSubmit']>>[0]

function getDefaultDataSourceName(): string {
    const opcAddress = loadPlcServerAddress().trim()
    if (!opcAddress) {
        return ''
    }
    const addressWithoutProtocol = opcAddress.replace(/^opc\.tcp:\/\//i, '')
    const host = addressWithoutProtocol.split(/[/:]/)[0]
    return host ? `Datasource_${host}` : ''
}

function SourcePage() {

    const navigate = useNavigate()
    const [sourceObject, setSourceObject] = useState<SourceObject>(() => ({
        DataSourceName: getDefaultDataSourceName(),
    }))
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
            const result: string = await createSource(sourceObject);
            
            saveSourceId(result);
            setSourceCreationSucceeded(true);
        } catch (error) {
            setErrorMessage(error instanceof Error ? error.message : 'Source creation failed. Please check your input and try again.')
        } finally {
            setIsSubmitting(false)
        }
    }

    return (
        <div className={Loginstyles.wrapper}>
            <form className={Loginstyles.loginForm} onSubmit={handleSubmit} noValidate>
                <h1>Create data source</h1>
                <FormField
                    id="sourceName"
                    label="Source name"
                    value={sourceObject.DataSourceName}
                    onChange={(value) => setSourceProperty('DataSourceName', value)}
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