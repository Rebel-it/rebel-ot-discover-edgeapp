import { useNavigate } from "react-router-dom";
import { savePlcAuth } from "../../services/sessionStorageService.ts";
import { connectToPlc } from "../../services/plcService.ts";
import type { PlcAuthObject } from "../../models/PlcAuthObject.ts";
import { useState } from "react";
import FormField from "../../components/molecules/formField/FormField.tsx";
import WizardPage from "../wizardPage/WizardPage.tsx";
import { useWizard } from "../../context/WizardContext.tsx";
import { Pages } from "../../models/Pages.ts";
import styles from "./PlcConnect.module.css";
import Checkbox from "../../components/atoms/checkbox/Checkbox.tsx";
import WarningTag from "../../components/atoms/warningTag/WarningTag.tsx";

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
  const [errorMessage, setErrorMessage] = useState("");
  const [opcUaServerAddressMissing, setOpcUaServerAddressMissing] = useState(false);
  const [opcUaUsernameMissing, setOpcUaUsernameMissing] = useState(false);
  const [opcUaPasswordMissing, setOpcUaPasswordMissing] = useState(false);

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

  function validFormInput() {
    let valid = true;

    if (!plcObject.OpcUaServerAddress.trim()) {
      setOpcUaServerAddressMissing(true);
      valid = false;
    } else {
      setOpcUaServerAddressMissing(false);
    }

    if (useCredentials) {
      if (!plcObject.OpcUaUsername.trim()) {
        setOpcUaUsernameMissing(true);
        valid = false;
      } else {
        setOpcUaUsernameMissing(false);
      }

      if (!plcObject.OpcUaPassword.trim()) {
        setOpcUaPasswordMissing(true);
        valid = false;
      } else {
        setOpcUaPasswordMissing(false);
      }
    }

    return valid;
  }

  async function handlePlcConnect() {
    if (!validFormInput()) {
      return;
    }

    setIsSubmitting(true);
    setErrorMessage("");

    try {
      await connectToPlc(plcObject);
      savePlcAuth(plcObject);
      markStepCompleted("plcConnect");
      navigate(Pages.source);
    } catch (error) {
      setErrorMessage(error instanceof Error ? error.message : "PLC connection failed. Please check your credentials and try again.");
    } finally {
      setIsSubmitting(false);
    }
  }

  return (
    <WizardPage
      wizardStep="plcConnect"
      title="PLC connection"
      continueButtonText="Connect"
      onContinue={handlePlcConnect}
      loading={isSubmitting}
    >
      <form className={styles.plcConnectForm} noValidate>
        <div className={styles.formFieldWrapper}>
          <FormField
            id="ipAddress"
            label="OPC Server Address"
            value={plcObject.OpcUaServerAddress}
            onChange={(value) => setPlcProperty("OpcUaServerAddress", value)}
            required
            placeholder="OPC://"
            invalidText={opcUaServerAddressMissing ? "OPC Server Address is required" : ""}
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
                placeholder="..."
                invalidText={opcUaUsernameMissing ? "OPC Username is required" : ""}
              />

              <FormField
                id="OpcUaPassword"
                label="OPC password"
                type="password"
                value={plcObject.OpcUaPassword}
                onChange={(value) => setPlcProperty("OpcUaPassword", value)}
                placeholder="..."
                invalidText={opcUaPasswordMissing ? "OPC Password is required" : ""}
              />
            </>
          )}
          {errorMessage && <WarningTag invalidText={errorMessage} />}
        </div>
      </form>
    </WizardPage>
  )
}

export default PlcConnect;