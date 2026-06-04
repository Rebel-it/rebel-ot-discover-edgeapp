import style from "./SourcePage.module.css"
import { useNavigate } from "react-router-dom"
import { useState } from "react"
import { loadPlcServerAddress, saveSourceId } from "../../services/sessionStorageService.ts"
import type { SourceObject } from "../../models/SourceObject.ts"
import { createSource } from "../../services/dataSourceService.ts"
import FormField from "../../components/molecules/formField/FormField.tsx"
import WizardPage from "../wizardPage/WizardPage.tsx"
import { Pages } from "../../models/Pages.ts"
import { useWizard } from "../../context/WizardContext.tsx"
import WizardPageTitle from "../../components/atoms/wizardPageTitle/WizardPageTitle.tsx"
import WarningTag from "../../components/atoms/warningTag/WarningTag.tsx"

function getDefaultDataSourceName(): string {
  const opcAddress = loadPlcServerAddress().trim();
  if (!opcAddress) {
    return "";
  }
  const addressWithoutProtocol = opcAddress.replace(/^opc\.tcp:\/\//i, "");
  const host = addressWithoutProtocol.split(/[/:]/)[0];
  return host ? `Datasource_${host}` : "";
}

const defaultSourceObject: SourceObject = {
  DataSourceName: getDefaultDataSourceName(),
}

function SourcePage() {
  const navigate = useNavigate();
  const { markStepCompleted } = useWizard();
  const [sourceObject, setSourceObject] = useState<SourceObject>(defaultSourceObject);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [errorMessage, setErrorMessage] = useState("");
  const [sourceIsMissing, setSourceIsMissing] = useState(false);

  function setSourceProperty<K extends keyof SourceObject>(property: K, value: SourceObject[K]) {
    setSourceObject((currentSourceObject) => ({
      ...currentSourceObject,
      [property]: value,
    }));
  }

  function validateFormInput() {
    if (!sourceObject.DataSourceName.trim()) {
      setSourceIsMissing(true);
      return false;
    } else {
      setSourceIsMissing(false);
    }

    return true;
  }

  async function handleCreateDataSource() {
    if (isSubmitting || !validateFormInput()) {
      return;
    }

    setIsSubmitting(true);
    setErrorMessage("");
    try {
      const result: string = await createSource(sourceObject);
      saveSourceId(result);
      markStepCompleted("source");
      navigate(Pages.variables);
    } catch (error) {
      setErrorMessage(error instanceof Error ? error.message : "Source creation failed. Please check your input and try again.");
    } finally {
      setIsSubmitting(false);
    }
  }

  return (
    <WizardPage
      wizardStep="source"
      continueButtonText="Create"
      onContinue={handleCreateDataSource}
      loading={isSubmitting}
    >
      <form className={style.sourceForm} noValidate>
        <WizardPageTitle title="Create data source" />

        <div className={style.formFieldWrapper}>
          <FormField
            id="sourceName"
            label="Source name"
            value={sourceObject.DataSourceName}
            onChange={(value) => setSourceProperty("DataSourceName", value)}
            placeholder="..."
            invalidText={sourceIsMissing ? "Source name is required" : ""}
          />
          {errorMessage && <WarningTag invalidText={errorMessage} />}

        </div>
      </form>
    </WizardPage>
  )
}

export default SourcePage;