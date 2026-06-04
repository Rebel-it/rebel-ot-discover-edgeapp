import { useState, useEffect, useRef } from "react"
import styles from "./TagPage.module.css"
import { createTags, getFilledTags, type Tag as ApiTag } from "../../services/tagService"
import { useNavigate } from "react-router-dom";
import WizardPage from "../wizardPage/WizardPage";
import { Pages } from "../../models/Pages";
import { useWizard } from "../../context/WizardContext";
import WizardPageTitle from "../../components/atoms/wizardPageTitle/WizardPageTitle";
import Table from "../../components/organisms/table/Table";
import { getFormulaLabel, getTagKey } from "../../models/Tag";
import { useTags } from "../../context/TagContext";

function TagPage() {
  const navigate = useNavigate();
  const { saveTags } = useTags();
  const { markStepCompleted } = useWizard();
  const [tags, setTags] = useState<ApiTag[]>([]);
  const [tagsLoading, setTagsLoading] = useState(true);
  const [tagsError, setTagsError] = useState("");
  const [selectedTagKeys, setSelectedTagKeys] = useState<Set<string>>(new Set());
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [errorMessage, setErrorMessage] = useState("");
  const selectAllRef = useRef<HTMLInputElement | null>(null);
  const isSubmittingRef = useRef(false);

  const tagKeys = tags.map(getTagKey);
  const allSelected = tagKeys.length > 0 && tagKeys.every((key) => selectedTagKeys.has(key));
  const someSelected = tagKeys.some((key) => selectedTagKeys.has(key));
  const selectedCount = selectedTagKeys.size;

  useEffect(() => {
    getFilledTags()
      .then(setTags)
      .catch(() => setTagsError("Unable to load prefilled tags. Check that the API is running."))
      .finally(() => setTagsLoading(false));
  }, [])

  useEffect(() => {
    if (selectAllRef.current) {
      selectAllRef.current.indeterminate = someSelected && !allSelected;
    }
  }, [someSelected, allSelected])

  function toggleTagSelection(tag: ApiTag) {
    const tagKey = getTagKey(tag);

    setSelectedTagKeys((prev) => {
      const next = new Set(prev);
      if (next.has(tagKey)) {
        next.delete(tagKey);
      } else {
        next.add(tagKey);
      }

      return next;
    });
  }

  function toggleSelectAll() {
    if (allSelected) {
      setSelectedTagKeys(new Set());
      return;
    }

    setSelectedTagKeys(new Set(tagKeys));
  }

  async function handleCreateTags() {
    if (selectedTagKeys.size === 0 || isSubmittingRef.current) {
      return;
    }

    isSubmittingRef.current = true;
    setIsSubmitting(true);
    setErrorMessage("");

    try {
      const selectedTags = tags.filter((tag) => selectedTagKeys.has(getTagKey(tag)));
      await createTags(selectedTags);

      saveTags(selectedTags);
      setSelectedTagKeys(new Set());
      markStepCompleted("tags");
      navigate(Pages.deviceConfig);
    } catch {
      setErrorMessage("Unable to reach the tag service. Check that the API is running.");
    } finally {
      isSubmittingRef.current = false;
      setIsSubmitting(false);
    }
  }

  const columnDefs = [
    { key: "name", label: "Name", sortable: true },
    { key: "logOn", label: "Log on", sortable: false },
    { key: "interval", label: "Interval", sortable: false },
    { key: "formula", label: "Formula", sortable: false },
  ]

  const rowData = tags.map((tag) => ({
    id: getTagKey(tag),
    cells: {
      name: tag.name,
      logOn: tag.logEvent,
      interval: tag.loggingInterval,
      formula: getFormulaLabel(tag)
    }
  }));

  return (
    <WizardPage
      wizardStep="tags"
      continueButtonText="I'm done creating tags"
      onContinue={handleCreateTags}
      loading={isSubmitting || selectedCount === 0}>

      <div className={styles.page}>
        <WizardPageTitle title="Tags" />

        <div className={styles.messageWrapper}>
          {tagsLoading && <p className={styles.empty}>Loading tags...</p>}
          {!tagsLoading && tagsError && <p className={styles.empty}>{tagsError}</p>}
          {!tagsLoading && !tagsError && tags.length === 0 && (
            <p className={styles.empty}>No prefilled tags available.</p>
          )}
          {errorMessage && <p>{errorMessage}</p>}
        </div>

        {/* {rowData.length > 0 && ( */}
        <div className={styles.tableWrapper}>
          <Table
            rows={rowData}
            columns={columnDefs}
            selectedIds={[]}
            onRowSelect={() => { }}
            onSelectAll={() => { }}
            onSort={() => { }}
          />
        </div>
        {/* )} */}
      </div>

      {/* 
      
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
                <th>Name</th>
                <th>Logging on</th>
                <th>Interval</th>
                <th>Formula</th>
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
                  <td>{tag.name}</td>
                  <td>{tag.logEvent}</td>
                  <td>{tag.loggingInterval}</td>
                  <td>{getFormulaLabel(tag)}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
      {errorMessage && <p>{errorMessage}</p>} */}
    </WizardPage>
  )
}

export default TagPage;
