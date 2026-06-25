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
      continueDisabled={isSubmitting}
    >
      <form className={styles.plcConnectForm} noValidate onSubmit={e => { e.preventDefault(); handlePlcConnect(); }}>
        <div className={styles.formFieldWrapper}>
          <FormField
            id="ipAddress"
            label="OPC UA Server Address"
            value={plcObject.OpcUaServerAddress}
            onChange={(value) => setPlcProperty("OpcUaServerAddress", value)}
            required
            placeholder="127.0.0.1:4840"
            invalidText={opcUaServerAddressMissing ? "OPC Server Address is required" : ""}
            prefix="opc.tcp://"
          />

          <Checkbox
            label="This server requires authentication"
            checked={useCredentials}
            onChange={handleUseCredentialsChange}
            theme="purple"
          />

          {useCredentials && (
            <>
              <FormField
                id="OpcUaUsername"
                label="Username required to access OPC UA server"
                value={plcObject.OpcUaUsername}
                onChange={(value) => setPlcProperty("OpcUaUsername", value)}
                invalidText={opcUaUsernameMissing ? "OPC Username is required" : ""}
              />

              <FormField
                id="OpcUaPassword"
                label="Password required to access OPC UA server"
                type="password"
                value={plcObject.OpcUaPassword}
                onChange={(value) => setPlcProperty("OpcUaPassword", value)}
                invalidText={opcUaPasswordMissing ? "OPC Password is required" : ""}
              />
            </>
          )}
          {errorMessage && <WarningTag invalidText={errorMessage} />}
        </div>
        <button type="submit" style={{ display: 'none' }} />
      </form>
    </WizardPage>
  )
}

export default PlcConnect;