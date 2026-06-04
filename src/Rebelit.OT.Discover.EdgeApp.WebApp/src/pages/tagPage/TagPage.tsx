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
import type { RowData } from "../../components/organisms/table/types";
import WarningTag from "../../components/atoms/warningTag/WarningTag";
import Spinner from "../../components/atoms/spinner/Spinner";

function TagPage() {
  const navigate = useNavigate();
  const { saveTags } = useTags();
  const { markStepCompleted } = useWizard();
  const [tags, setTags] = useState<ApiTag[]>([]);
  const [tagsLoading, setTagsLoading] = useState(true);
  const [tagsError, setTagsError] = useState("");
  const [selectedTagKeys, setSelectedTagKeys] = useState<string[]>([]);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [errorMessage, setErrorMessage] = useState("");
  const [sortedRows, setSortedRows] = useState<RowData[]>([]);
  const selectAllRef = useRef<HTMLInputElement | null>(null);
  const [hasSubmitted, setHasSubmitted] = useState(false);

  const tagKeys = tags.map(getTagKey);
  const allSelected = tagKeys.length > 0 && tagKeys.every((key) => selectedTagKeys.includes(key));
  const someSelected = tagKeys.some((key) => selectedTagKeys.includes(key));
  const selectedCount = selectedTagKeys.length;

  useEffect(() => {
    getFilledTags()
      .then((fetchedTags) => {
        setTags(fetchedTags);
        const rowData = fetchedTags.map((tag) => ({
          id: getTagKey(tag),
          cells: {
            name: tag.name,
            logOn: tag.logEvent,
            interval: tag.loggingInterval,
            formula: getFormulaLabel(tag)
          }
        }));
        setSortedRows(rowData);
      })
      .catch(() => setTagsError("Unable to load prefilled tags. Check that the API is running."))
      .finally(() => setTagsLoading(false));
  }, [])

  useEffect(() => {
    if (selectAllRef.current) {
      selectAllRef.current.indeterminate = someSelected && !allSelected;
    }
  }, [someSelected, allSelected])

  function toggleTagSelection(tagKey: string) {
    setSelectedTagKeys((prev) => {
      const next = [...prev];
      const index = next.indexOf(tagKey);
      if (index !== -1) {
        next.splice(index, 1);
      } else {
        next.push(tagKey);
      }

      return next;
    });
  }

  function toggleSelectAll() {
    if (allSelected) {
      setSelectedTagKeys([]);
      return;
    }

    setSelectedTagKeys([...tagKeys]);
  }

  async function handleCreateTags() {
    setHasSubmitted(true);
    
    if (selectedTagKeys.length === 0 || isSubmitting) {
      return;
    }

    setIsSubmitting(true);
    setErrorMessage("");

    try {
      const selectedTags = tags.filter((tag) => selectedTagKeys.includes(getTagKey(tag)));
      await createTags(selectedTags);

      saveTags(selectedTags);
      setSelectedTagKeys([]);
      markStepCompleted("tags");
      navigate(Pages.deviceConfig);
    } catch {
      setErrorMessage("Unable to reach the tag service. Check that the API is running.");
    } finally {
      setIsSubmitting(false);
    }
  }

  const noTagsAvailable = !tagsLoading && tags.length === 0 && !tagsError;

  function buttonClickable() {
    if (noTagsAvailable) {
      return true;
    }

    if (isSubmitting || selectedCount === 0) {
      return false;
    }
    return true;
  }

  const columnDefs = [
    { key: "name", label: "Name", sortable: true },
    { key: "logOn", label: "Log on", sortable: false },
    { key: "interval", label: "Interval", sortable: false },
    { key: "formula", label: "Formula", sortable: false },
  ]

  return (
    <WizardPage
      wizardStep="tags"
      continueButtonText={noTagsAvailable ? "Finish" : "Create tags"}
      onContinue={noTagsAvailable ? () => navigate(Pages.start) : handleCreateTags}
      loading={!buttonClickable()}>

      <div className={styles.page}>
        <WizardPageTitle title="Tags" />

        <div className={styles.statusIndicatorWrapper}>
          {(tagsLoading || isSubmitting) && <Spinner />}

          {!tagsLoading && tagsError && <WarningTag invalidText={tagsError} />}
          {noTagsAvailable && (
            <WarningTag invalidText={"No tags available"} />
          )}

          {errorMessage && <WarningTag invalidText={errorMessage} />}
        </div>

        {sortedRows.length > 0 && (
          <div className={styles.tableWrapper}>
            <Table
              rows={sortedRows}
              columns={columnDefs}
              selectedIds={selectedTagKeys}
              onSelectAll={toggleSelectAll}
              onRowSelect={(id) => toggleTagSelection(id)}
              onSort={() => {
                const newRows = [...sortedRows].sort((a, b) => a.cells.name.localeCompare(b.cells.name));
                setSortedRows(newRows);
              }}
            />
          </div>
        )}

        {hasSubmitted && selectedCount === 0 && (
          <WarningTag invalidText={"No tags selected"} />
        )}

      </div>
    </WizardPage>
  )
}

export default TagPage;
