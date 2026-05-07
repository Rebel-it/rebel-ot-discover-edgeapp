import { useState, useEffect, useRef } from 'react'
import styles from './TagPage.module.css'
import { createTag, type CreateTagRequest } from '../../services/TagService'
import { getVariables, type VariableOption } from '../../services/VariableService'

const LOGGING_ON_OPTIONS = ['Interval', 'Change']

type Option = { label: string; value: string }

const INTERVAL_OPTIONS: Option[] = [
    { label: 'Every 200 milliseconds', value: '200ms' },
    { label: 'Every 500 milliseconds', value: '500ms' },
    { label: 'Every second',           value: '1s'    },
    { label: 'Every 10 seconds',       value: '10s'   },
    { label: 'Every 30 seconds',       value: '30s'   },
    { label: 'Every minute',           value: '1m'    },
    { label: 'Every 5 minutes',        value: '5m'    },
    { label: 'Every 15 minutes',       value: '15m'   },
    { label: 'Every 30 minutes',       value: '30m'   },
    { label: 'Every hour',             value: '1h'    },
    { label: 'Custom',                 value: 'custom'},
]

const RATE_LIMIT_OPTIONS: Option[] = [
    { label: 'Up to 20000 values per hour', value: '180ms'  },
    { label: 'Up to 5000 values per hour',  value: '720ms'  },
    { label: 'Up to 1000 values per hour',  value: '3600ms' },
    { label: 'Up to 500 values per hour',   value: '7200ms' },
    { label: 'Up to 100 values per hour',   value: '36s'    },
    { label: 'Up to 50 values per hour',    value: '72s'    },
    { label: 'Up to 25 values per hour',    value: '144s'   },
    { label: 'Up to 10 values per hour',    value: '6m'     },
    { label: 'Up to 5 values per hour',     value: '12m'    },
]

const SPECIFICATION_OPTIONS: Option[] = [
    { label: 'On value changes only (default)',                    value: 'value_changes_only'    },
    { label: 'On value changes and each hour of unchanged value',  value: 'value_changes_hourly'  },
]

const FORMULA_OPTIONS: Option[] = [
    { label: 'LAST (default)', value: '' },
]

const RETENTION_OPTIONS: Option[] = [
    { label: '3 days',   value: '3d' },
    { label: '7 days',   value: '7d' },
    { label: '2 weeks',  value: '14d' },
    { label: '6 months', value: '26w' },
    { label: '2 years',  value: '104w' },
    { label: '5 years',  value: '260w' },
    { label: '7 years',  value: '364w' },
]

type LoggingOn = 'Interval' | 'Change'

type Tag = {
    variable: string
    variablePublicId: string
    name: string
    identifier: string
    loggingOn: '' | LoggingOn
    interval: string
    customInterval: string
    formula: string
    retention: string
}

const emptyTag = (): Tag => ({
    variable: '',
    variablePublicId: '',
    name: '',
    identifier: '',
    loggingOn: '',
    interval: '',
    customInterval: '',
    formula: '',
    retention: '',
})

function TagPage() {
    const [tags, setTags] = useState<Tag[]>([])
    const [isModalOpen, setIsModalOpen] = useState(false)
    const [draft, setDraft] = useState<Tag>(emptyTag())
    const [variables, setVariables] = useState<VariableOption[]>([])
    const [variablesLoading, setVariablesLoading] = useState(false)
    const [isSubmitting, setIsSubmitting] = useState(false)
    const [errorMessage, setErrorMessage] = useState('')
    const dialogRef = useRef<HTMLDialogElement>(null)
    const isSubmittingRef = useRef(false)

    const isChange = draft.loggingOn === 'Change'
    const intervalOptions = isChange ? RATE_LIMIT_OPTIONS : INTERVAL_OPTIONS
    const intervalLabel = isChange ? 'Rate limit' : 'Interval'
    const formulaLabel = isChange ? 'Specification' : 'Formula'
    const formulaOptions = isChange ? SPECIFICATION_OPTIONS : FORMULA_OPTIONS

    useEffect(() => {
        const dialog = dialogRef.current
        if (!dialog) return

        if (isModalOpen) {
            if (!dialog.open) {
                dialog.showModal()
            }
        } else if (dialog.open) {
            dialog.close()
        }
    }, [isModalOpen])

    useEffect(() => {
        if (!isModalOpen) return
        getVariables().then(setVariables).finally(() => setVariablesLoading(false))
    }, [isModalOpen])

    function setDraftField<K extends keyof Tag>(key: K, value: Tag[K]) {
        setDraft((prev) => ({ ...prev, [key]: value }))
    }

    function buildCreateTagRequest(tag: Tag): CreateTagRequest | null {
        switch (tag.loggingOn) {
            case 'Interval':
                return {
                    logEvent: 'interval',
                    loggingInterval: tag.interval === 'custom' ? tag.customInterval : tag.interval,
                    name: tag.name,
                    onChangeExpiry: null,
                    retentionPolicy: tag.retention,
                    slug: tag.identifier,
                    variable: tag.variablePublicId,
                    edgeAggregator: tag.formula && tag.formula !== 'last' ? tag.formula : null,
                }
            case 'Change':
                return {
                    logEvent: 'change',
                    loggingInterval: tag.interval,
                    name: tag.name,
                    onChangeExpiry: tag.formula === 'value_changes_hourly' ? '1h' : null,
                    retentionPolicy: tag.retention,
                    slug: tag.identifier,
                    variable: tag.variablePublicId,
                    edgeAggregator: null,
                }
            default:
                return null
        }
    }

    function handleLoggingOnChange(value: Tag['loggingOn']) {
        setDraft((prev) => ({ ...prev, loggingOn: value, interval: '', formula: value === 'Interval' ? 'last' : '' }))
    }

    function handleVariableChange(publicId: string) {
        const selectedVariable = variables.find((variable) => variable.publicId === publicId)

        setDraft((prev) => ({
            ...prev,
            variable: selectedVariable?.label ?? '',
            variablePublicId: publicId,
        }))
    }

    function openModal() {
        setDraft(emptyTag())
        setVariables([])
        setVariablesLoading(true)
        setErrorMessage('')
        setIsModalOpen(true)
    }

    function closeModal() {
        setIsModalOpen(false)
    }

    async function handleSave() {
        const request = buildCreateTagRequest(draft)
        if (!request || isSubmittingRef.current) {
            return
        }

        isSubmittingRef.current = true
        setIsSubmitting(true)
        setErrorMessage('')

        try {
            const response = await createTag(request)
            if (!response.ok) {
                setErrorMessage('Unable to save tag. Check the values and try again.')
                return
            }

            setTags((prev) => [...prev, draft])
            setIsModalOpen(false)
        } catch {
            setErrorMessage('Unable to reach the tag service. Check that the API is running.')
        } finally {
            isSubmittingRef.current = false
            setIsSubmitting(false)
        }
    }

    return (
        <div className={styles.pageWrapper}>
            <div className={styles.header}>
                <h1>Tags</h1>
                <button type="button" className={styles.addButton} onClick={openModal}>
                    +
                </button>
            </div>

            {tags.length === 0 ? (
                <p className={styles.empty}>No tags yet. Click + to add one.</p>
            ) : (
                <div className={styles.tableWrapper}>
                    <table className={styles.table}>
                        <thead>
                            <tr>
                                <th>Variable</th>
                                <th>Name</th>
                                <th>Identifier</th>
                                <th>Logging on</th>
                                <th>Interval</th>
                                <th>Formula</th>
                                <th>Retention</th>
                            </tr>
                        </thead>
                        <tbody>
                            {tags.map((tag) => (
                                <tr key={`${tag.identifier}-${tag.variable}`}>
                                    <td>{tag.variable}</td>
                                    <td>{tag.name}</td>
                                    <td>{tag.identifier}</td>
                                    <td>{tag.loggingOn}</td>
                                    <td>{tag.interval === 'custom' ? tag.customInterval : tag.interval}</td>
                                    <td>{tag.formula}</td>
                                    <td>{tag.retention}</td>
                                </tr>
                            ))}
                        </tbody>
                    </table>
                </div>
            )}

            {isModalOpen && (
                  <dialog ref={dialogRef} className={styles.modal} onClose={closeModal} aria-labelledby="modal-title">
                    <h2 id="modal-title">Add tag</h2>

                    <div className={styles.formField}>
                        <label htmlFor="variable">Variable</label>
                        <select
                            id="variable"
                            value={draft.variablePublicId}
                            onChange={(e) => handleVariableChange(e.target.value)}
                            disabled={variablesLoading}
                        >
                            <option value="">{variablesLoading ? 'Loading...' : 'Select a variable'}</option>
                            {variables.map((v) => (
                                <option key={v.publicId} value={v.publicId}>{v.label}</option>
                            ))}
                        </select>
                    </div>

                    <div className={styles.formField}>
                        <label htmlFor="name">Name</label>
                        <input id="name" value={draft.name} onChange={(e) => setDraftField('name', e.target.value)} />
                    </div>

                    <div className={styles.formField}>
                        <label htmlFor="identifier">Identifier</label>
                        <input id="identifier" value={draft.identifier} onChange={(e) => setDraftField('identifier', e.target.value)} />
                    </div>

                    <div className={styles.formField}>
                        <label htmlFor="loggingOn">Logging on</label>
                        <select
                            id="loggingOn"
                            value={draft.loggingOn}
                            onChange={(e) => handleLoggingOnChange(e.target.value as Tag['loggingOn'])}
                        >
                            <option value="">select an option</option>
                            {LOGGING_ON_OPTIONS.map((v) => (
                                <option key={v} value={v}>{v}</option>
                            ))}
                        </select>
                    </div>

                    <div className={styles.formField}>
                        <label htmlFor="interval">{intervalLabel}</label>
                        <select
                            id="interval"
                            value={draft.interval}
                            onChange={(e) => setDraftField('interval', e.target.value)}
                        >
                            <option value="">select an option</option>
                            {intervalOptions.map((o) => (
                                <option key={o.value} value={o.value}>{o.label}</option>
                            ))}
                        </select>
                    </div>

                    {!isChange && draft.interval === 'custom' && (
                        <div className={styles.formField}>
                            <label htmlFor="customInterval">Custom interval</label>
                            <input
                                id="customInterval"
                                value={draft.customInterval}
                                onChange={(e) => setDraftField('customInterval', e.target.value)}
                                placeholder="e.g. 45 seconds"
                            />
                        </div>
                    )}

                    <div className={styles.formField}>
                        <label htmlFor="formula">{formulaLabel}</label>
                        <select
                            id="formula"
                            value={draft.formula}
                            onChange={(e) => setDraftField('formula', e.target.value)}
                        >
                            <option value="">select an option</option>
                            {formulaOptions.map((o) => (
                                <option key={o.value} value={o.value}>{o.label}</option>
                            ))}
                        </select>
                    </div>

                    <div className={styles.formField}>
                        <label htmlFor="retention">Retention</label>
                        <select
                            id="retention"
                            value={draft.retention}
                            onChange={(e) => setDraftField('retention', e.target.value)}
                        >
                            <option value="">select a retention</option>
                            {RETENTION_OPTIONS.map((o) => (
                                <option key={o.value} value={o.value}>{o.label}</option>
                            ))}
                        </select>
                    </div>

                    {errorMessage && <p>{errorMessage}</p>}

                    <div className={styles.modalActions}>
                        <button type="button" className={styles.cancelButton} onClick={closeModal}>
                            Cancel
                        </button>
                        <button type="button" className={styles.saveButton} onClick={handleSave} disabled={isSubmitting}>
                            {isSubmitting ? 'Saving...' : 'Save'}
                        </button>
                    </div>
                </dialog>
            )}
        </div>
    )
}

export default TagPage
