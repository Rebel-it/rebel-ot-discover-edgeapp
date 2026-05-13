import { useState, useEffect, useRef } from 'react'
import styles from './TagPage.module.css'
import DropDown, { type DropDownOption } from '../../components/Atoms/DropDown/DropDown'
import { createTag, updateTag, getTags, type CreateTagRequest, type Tag as ApiTag } from '../../services/TagService'
import { getVariables, type VariableOption } from '../../services/VariableService'
import {
    LOGGING_ON_OPTIONS,
    INTERVAL_OPTIONS,
    RATE_LIMIT_OPTIONS,
    SPECIFICATION_OPTIONS,
    FORMULA_OPTIONS,
    RETENTION_OPTIONS,
} from './TagPage.constants'
import RoundButton from '../../components/Atoms/RoundButton/RoundButton'

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

function mapApiTagToTag(apiTag: ApiTag): Tag {
    let loggingOn: Tag['loggingOn'] = ''

    if (apiTag.logEvent === 'interval') {
        loggingOn = 'Interval'
    } else if (apiTag.logEvent === 'change') {
        loggingOn = 'Change'
    }

    let formula = apiTag.edgeAggregator ?? ''

    if (apiTag.logEvent === 'change') {
        formula = apiTag.onChangeExpiry === '1h' ? 'value_changes_hourly' : 'value_changes_only'
    }

    return {
        variable: apiTag.variable.publicId,
        variablePublicId: apiTag.variable.publicId,
        name: apiTag.name,
        identifier: apiTag.slug,
        loggingOn,
        interval: apiTag.loggingInterval,
        customInterval: '',
        formula,
        retention: apiTag.retentionPolicy,
    }
}

function TagPage() {
    const [tags, setTags] = useState<Tag[]>([])
    const [tagsLoading, setTagsLoading] = useState(true)
    const [tagsError, setTagsError] = useState('')
    const [isModalOpen, setIsModalOpen] = useState(false)
    const [draft, setDraft] = useState<Tag>(emptyTag())
    const [variables, setVariables] = useState<VariableOption[]>([])
    const [variablesLoading, setVariablesLoading] = useState(false)
    const [isSubmitting, setIsSubmitting] = useState(false)
    const [errorMessage, setErrorMessage] = useState('')
    const [editingIdentifier, setEditingIdentifier] = useState<string | null>(null)
    const dialogRef = useRef<HTMLDialogElement>(null)
    const isSubmittingRef = useRef(false)

    const isChange = draft.loggingOn === 'Change'
    const intervalOptions = isChange ? RATE_LIMIT_OPTIONS : INTERVAL_OPTIONS
    const intervalLabel = isChange ? 'Rate limit' : 'Interval'
    const formulaLabel = isChange ? 'Specification' : 'Formula'
    const formulaOptions = isChange ? SPECIFICATION_OPTIONS : FORMULA_OPTIONS
    const variableOptions: DropDownOption[] = variables.map((variable) => ({
        label: variable.label,
        value: variable.publicId,
    }))
    const loggingOnOptions: DropDownOption[] = LOGGING_ON_OPTIONS.map((value) => ({ label: value, value }))

    useEffect(() => {
        getTags()
            .then((loadedTags) => setTags(loadedTags.map(mapApiTagToTag)))
            .catch(() => setTagsError('Unable to load tags. Check that the API is running.'))
            .finally(() => setTagsLoading(false))
    }, [])

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
        setEditingIdentifier(null)
        setVariables([])
        setVariablesLoading(true)
        setErrorMessage('')
        setIsModalOpen(true)
    }

    function openEditModal(tag: Tag) {
        setDraft(tag)
        setEditingIdentifier(tag.identifier)
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
            if (editingIdentifier) {
                await updateTag(editingIdentifier, request)
                setTags((prev) => prev.map((t) => t.identifier === editingIdentifier ? draft : t))
            } else {
                await createTag(request)
                setTags((prev) => [...prev, draft])
            }
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

            {tagsLoading && <p className={styles.empty}>Loading tags...</p>}
            {!tagsLoading && tagsError && <p className={styles.empty}>{tagsError}</p>}
            {!tagsLoading && !tagsError && tags.length === 0 && (
                <p className={styles.empty}>No tags yet. Click + to add one.</p>
            )}
            {!tagsLoading && !tagsError && tags.length > 0 && (
                <div className={styles.tableWrapper}>
                    <table className={styles.table}>
                        <thead>
                            <tr>
                                <th>PublicId</th>
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
                                    <td><RoundButton onClick={() => openEditModal(tag)}> Edit</RoundButton></td>
                                </tr>
                            ))}
                        </tbody>
                    </table>
                </div>
            )}

            {isModalOpen && (
                  <dialog ref={dialogRef} className={styles.modal} onClose={closeModal} aria-labelledby="modal-title">
                    <h2 id="modal-title">{editingIdentifier ? 'Edit tag' : 'Add tag'}</h2>

                    <DropDown
                        id="variable"
                        label="Variable"
                        value={draft.variablePublicId}
                        options={variableOptions}
                        onChange={handleVariableChange}
                        disabled={variablesLoading}
                        placeholder={variablesLoading ? 'Loading...' : 'Select a variable'}
                    />

                    <div className={styles.formField}>
                        <label htmlFor="name">Name</label>
                        <input id="name" value={draft.name} onChange={(e) => setDraftField('name', e.target.value)} />
                    </div>

                    <div className={styles.formField}>
                        <label htmlFor="identifier">Identifier</label>
                        <input id="identifier" value={draft.identifier} onChange={(e) => setDraftField('identifier', e.target.value)} />
                    </div>

                    <DropDown
                        id="loggingOn"
                        label="Logging on"
                        value={draft.loggingOn}
                        options={loggingOnOptions}
                        onChange={(value) => handleLoggingOnChange(value as Tag['loggingOn'])}
                        placeholder="select an option"
                    />

                    <DropDown
                        id="interval"
                        label={intervalLabel}
                        value={draft.interval}
                        options={intervalOptions}
                        onChange={(value) => setDraftField('interval', value)}
                        placeholder="select an option"
                    />

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

                    <DropDown
                        id="formula"
                        label={formulaLabel}
                        value={draft.formula}
                        options={formulaOptions}
                        onChange={(value) => setDraftField('formula', value)}
                        placeholder="select an option"
                    />

                    <DropDown
                        id="retention"
                        label="Retention"
                        value={draft.retention}
                        options={RETENTION_OPTIONS}
                        onChange={(value) => setDraftField('retention', value)}
                        placeholder="select a retention"
                    />

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
