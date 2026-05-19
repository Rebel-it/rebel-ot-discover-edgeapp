import { useState, useEffect, useRef } from 'react'
import styles from './TagPage.module.css'
import { createTags, getFilledTags, type Tag as ApiTag } from '../../services/TagService'

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
    const [selectedTagKeys, setSelectedTagKeys] = useState<Set<string>>(new Set())
    const [isSubmitting, setIsSubmitting] = useState(false)
    const [errorMessage, setErrorMessage] = useState('')
    const selectAllRef = useRef<HTMLInputElement | null>(null)
    const isSubmittingRef = useRef(false)

    const tagKeys = tags.map(getTagKey)
    const allSelected = tagKeys.length > 0 && tagKeys.every((key) => selectedTagKeys.has(key))
    const someSelected = tagKeys.some((key) => selectedTagKeys.has(key))
    const selectedCount = selectedTagKeys.size

    useEffect(() => {
        getFilledTags()
            .then((loadedTags) => setTags(loadedTags.map(mapApiTagToTag)))
            .catch(() => setTagsError('Unable to load prefilled tags. Check that the API is running.'))
            .finally(() => setTagsLoading(false))
    }, [])

    useEffect(() => {
        if (selectAllRef.current) {
            selectAllRef.current.indeterminate = someSelected && !allSelected
        }
    }, [someSelected, allSelected])

    function buildCreateTagRequest(tag: Tag): ApiTag | null {
        switch (tag.loggingOn) {
            case 'Interval':
                return {
                    logEvent: 'interval',
                    loggingInterval: tag.interval === 'custom' ? tag.customInterval : tag.interval,
                    name: tag.name,
                    onChangeExpiry: null,
                    retentionPolicy: tag.retention,
                    slug: tag.identifier,
                    variable: { publicId: tag.variablePublicId },
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
                    variable: { publicId: tag.variablePublicId },
                    edgeAggregator: null,
                }
            default:
                return null
        }
    }

    function getTagKey(tag: Tag) {
        return `${tag.identifier}-${tag.variable}`
    }

    function toggleTagSelection(tag: Tag) {
        const tagKey = getTagKey(tag)

        setSelectedTagKeys((prev) => {
            const next = new Set(prev)
            if (next.has(tagKey)) {
                next.delete(tagKey)
            } else {
                next.add(tagKey)
            }

            return next
        })
    }

    function toggleSelectAll() {
        if (allSelected) {
            setSelectedTagKeys(new Set())
            return
        }

        setSelectedTagKeys(new Set(tagKeys))
    }

    async function handleCreateSelected() {
        if (selectedTagKeys.size === 0 || isSubmittingRef.current) {
            return
        }

        isSubmittingRef.current = true
        setIsSubmitting(true)
        setErrorMessage('')

        try {
            const selectedTags = tags.filter((tag) => selectedTagKeys.has(getTagKey(tag)))
            const requests = selectedTags
                .map(buildCreateTagRequest)
                .filter((request): request is ApiTag => request !== null)

            await createTags(requests)

            setTags((prev) => prev.filter((tag) => !selectedTagKeys.has(getTagKey(tag))))
            setSelectedTagKeys(new Set())
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
                <button
                    type="button"
                    className={styles.saveButton}
                    onClick={handleCreateSelected}
                    disabled={isSubmitting || selectedCount === 0}
                >
                    {isSubmitting ? 'Creating...' : `Create selected (${selectedCount})`}
                </button>
            </div>

            {tagsLoading && <p className={styles.empty}>Loading tags...</p>}
            {!tagsLoading && tagsError && <p className={styles.empty}>{tagsError}</p>}
            {!tagsLoading && !tagsError && tags.length === 0 && (
                <p className={styles.empty}>No prefilled tags available.</p>
            )}
            {!tagsLoading && !tagsError && tags.length > 0 && (
                <div className={styles.tableWrapper}>
                    <table className={styles.table}>
                        <thead>
                            <tr>
                                <th aria-label="Select all tags">
                                    <input
                                        ref={selectAllRef}
                                        type="checkbox"
                                        checked={allSelected}
                                        onChange={toggleSelectAll}
                                        aria-label="Select all tags"
                                    />
                                </th>
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
                                <tr key={getTagKey(tag)}>
                                    <td>
                                        <input
                                            type="checkbox"
                                            checked={selectedTagKeys.has(getTagKey(tag))}
                                            onChange={() => toggleTagSelection(tag)}
                                            aria-label={`Select tag ${tag.name || tag.identifier}`}
                                        />
                                    </td>
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

            {errorMessage && <p>{errorMessage}</p>}
        </div>
    )
}

export default TagPage
