import { useState, useEffect, useRef } from 'react'
import styles from './TagPage.module.css'
import sharedStyles from '../loginPage/LoginPage.module.css'
import { createTags, getFilledTags, type Tag as ApiTag } from '../../services/tagService'
import { useNavigate } from 'react-router-dom';

function TagPage() {
  const navigate = useNavigate();
  const [tags, setTags] = useState<ApiTag[]>([])
  const [tagsLoading, setTagsLoading] = useState(true)
  const [tagsError, setTagsError] = useState('')
  const [selectedTagKeys, setSelectedTagKeys] = useState<Set<string>>(new Set())
  const [isSubmitting, setIsSubmitting] = useState(false)
  const [errorMessage, setErrorMessage] = useState('')
  const selectAllRef = useRef<HTMLInputElement | null>(null)
  const isSubmittingRef = useRef(false)
  const [initialTagsCreated, setInitialTagsCreated] = useState(false);

  const tagKeys = tags.map(getTagKey)
  const allSelected = tagKeys.length > 0 && tagKeys.every((key) => selectedTagKeys.has(key))
  const someSelected = tagKeys.some((key) => selectedTagKeys.has(key))
  const selectedCount = selectedTagKeys.size

  useEffect(() => {
    getFilledTags()
      .then(setTags)
      .catch(() => setTagsError('Unable to load prefilled tags. Check that the API is running.'))
      .finally(() => setTagsLoading(false))
  }, [])

  useEffect(() => {
    if (selectAllRef.current) {
      selectAllRef.current.indeterminate = someSelected && !allSelected
    }
  }, [someSelected, allSelected])

  function getTagKey(tag: ApiTag) {
    return `${tag.slug}-${tag.variable.publicId}`
  }

  function toggleTagSelection(tag: ApiTag) {
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

  function getFormulaLabel(tag: ApiTag) {
    if (tag.logEvent === 'change') {
      return tag.onChangeExpiry === '1h' ? 'value_changes_hourly' : 'value_changes_only'
    }

    return tag.edgeAggregator ?? ''
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
      await createTags(selectedTags)

      setTags((prev) => prev.filter((tag) => !selectedTagKeys.has(getTagKey(tag))))
      setSelectedTagKeys(new Set())
      setInitialTagsCreated(true)
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
                      aria-label={`Select tag ${tag.name || tag.slug}`}
                    />
                  </td>
                  <td>{tag.variable.publicId}</td>
                  <td>{tag.name}</td>
                  <td>{tag.slug}</td>
                  <td>{tag.logEvent}</td>
                  <td>{tag.loggingInterval}</td>
                  <td>{getFormulaLabel(tag)}</td>
                  <td>{tag.retentionPolicy}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
      
      {initialTagsCreated && (
        <button type="button" className={sharedStyles.nextButton}  onClick={() => navigate('/final')}>
          Finish
        </button>
      )}

      {errorMessage && <p>{errorMessage}</p>}
    </div>
  )
}

export default TagPage
