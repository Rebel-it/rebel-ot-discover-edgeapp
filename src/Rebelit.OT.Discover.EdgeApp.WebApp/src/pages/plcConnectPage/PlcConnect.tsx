import { useNavigate } from "react-router-dom"
import { savePlcAuth } from "../../services/sessionStorageService.ts"
import { connectToPlc } from "../../services/plcService.ts"
import type { PlcAuthObject } from "../../models/PlcAuthObject.ts"
import { useState, type ComponentProps } from "react"
import FormField from "../../components/atoms/formField/FormField.tsx"
import WizardPage from "../wizardPage/WizardPage.tsx"
import { useWizard } from "../../context/WizardContext.tsx"
import { Pages } from "../../models/Pages.ts"
import WizardPageTitle from "../../components/atoms/wizardPageTitle/WizardPageTitle.tsx"
import styles from "./PlcConnect.module.css"
import Checkbox from "../../components/atoms/checkbox/Checkbox.tsx"

type PlcFormSubmitEvent = Parameters<NonNullable<ComponentProps<"form">["onSubmit"]>>[0]

const defaultPlcObject: PlcAuthObject = {
  OpcUaServerAddress: "",
  OpcUaUsername: "",
  OpcUaPassword: "",
}

function PlcConnect() {
  const navigate = useNavigate();
  const { markStepCompleted } = useWizard();
  const [plcObject, setPlcObject] = useState<PlcAuthObject>(defaultPlcObject);
  const [useCredentials, setUseCredentials] = useState(false);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [connectionSucceeded, setConnectionSucceeded] = useState(false);
  const [errorMessage, setErrorMessage] = useState("");

  function setPlcProperty<K extends keyof PlcAuthObject>(property: K, value: PlcAuthObject[K]) {
    setPlcObject((currentPlcObject) => ({
      ...currentPlcObject,
      [property]: value,
    }));
  }

  function handleUseCredentialsChange(checked: boolean) {
    setUseCredentials(checked);

    if (!checked) {
      setPlcObject((currentPlcObject) => ({
        ...currentPlcObject,
        OpcUaUsername: "",
        OpcUaPassword: "",
      }));
    }
  }

  async function handleSubmit(event: PlcFormSubmitEvent) {
    event.preventDefault();
    setIsSubmitting(true);
    setErrorMessage("");
    setConnectionSucceeded(false);

    try {
      await connectToPlc(plcObject);
      savePlcAuth(plcObject);
      setConnectionSucceeded(true);
    } catch (error) {
      setErrorMessage(error instanceof Error ? error.message : "PLC connection failed. Please check your credentials and try again.");
    } finally {
      setIsSubmitting(false);
    }
  }

  return (
    <WizardPage
      wizardStep="plcConnect"
      continueButtonText="Connect"
      onContinue={() => {
        markStepCompleted("plcConnect");
        navigate(Pages.source);
      }}
    >
      <form className={styles.plcConnectForm} onSubmit={handleSubmit} noValidate>
        <WizardPageTitle title="PLC connection" />

        <div className={styles.formFieldWrapper}>
          <FormField
            id="ipAddress"
            label="OPC Server Address"
            value={plcObject.OpcUaServerAddress}
            onChange={(value) => setPlcProperty("OpcUaServerAddress", value)}
            required
          />

          <Checkbox
            label="Use PLC username and password"
            checked={useCredentials}
            onChange={handleUseCredentialsChange}
          />

          {useCredentials && (
            <>
              <FormField
                id="OpcUaUsername"
                label="OPC username"
                value={plcObject.OpcUaUsername}
                onChange={(value) => setPlcProperty("OpcUaUsername", value)}
              />

              <FormField
                id="OpcUaPassword"
                label="OPC password"
                type="password"
                value={plcObject.OpcUaPassword}
                onChange={(value) => setPlcProperty("OpcUaPassword", value)}
              />
            </>
          )}
        </div>

        {errorMessage && <p className={`${styles.formMessage} ${styles.errorMessage}`}>{errorMessage}</p>}

        {connectionSucceeded && (
          <p className={`${styles.formMessage} ${styles.successMessage}`}>
            PLC connection succeeded. Continue to the next step.
          </p>
        )}
      </form>
    </WizardPage>
  )
}

export default PlcConnect;