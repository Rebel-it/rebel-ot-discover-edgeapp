import { useEffect, useState } from "react"
import { saveIxonAuth, clearIxonAuthenticationHeaders } from "../../services/sessionStorageService.ts"
import styles from "./LoginPage.module.css"
import { useNavigate } from "react-router-dom"
import { getCompanyConfiguration } from "../../services/companyConfigurationService.ts"
import type { ServiceAccountObject } from "../../models/ServiceAccountObject.ts"
import FormField from "../../components/molecules/formField/FormField.tsx"
import WizardPage from "../wizardPage/WizardPage.tsx"
import { useWizard } from "../../context/WizardContext.tsx"
import { Pages } from "../../models/Pages.ts"
import WarningTag from "../../components/atoms/warningTag/WarningTag.tsx"
import Modal from "../../components/modals/Modal.tsx"
import Video from "../../components/atoms/video/Video.tsx"
import VideoPreview from "../../components/atoms/videoPreview/VideoPreview.tsx"

const defaultAuthObject: ServiceAccountObject = {
  apiApplicationID: "",
  accessToken: ""
}

function LoginPage() {
  const navigate = useNavigate();
  const { markStepCompleted, deleteCompletedSteps } = useWizard();
  const [serviceAccount, setServiceAccount] = useState<ServiceAccountObject>(defaultAuthObject);
  const [errorMessage, setErrorMessage] = useState<string>("");
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [applicationIdMissing, setapplicationIdMissing] = useState(false);
  const [accessTokenMissing, setAccessTokenMissing] = useState(false);
  const [videoOpen, setVideoOpen] = useState(false);

  useEffect(() => {
    deleteCompletedSteps();
  }, []);

  function setAuthProperty<K extends keyof ServiceAccountObject>(property: K, value: ServiceAccountObject[K]) {
    setServiceAccount((currentServiceAccount) => ({
      ...currentServiceAccount,
      [property]: value,
    }));
  }

  function validFormInput() {
    let valid = true;

    if (!serviceAccount.apiApplicationID.trim()) {
      setapplicationIdMissing(true);
      valid = false;
    } else {
      setapplicationIdMissing(false);
    }

    if (!serviceAccount.accessToken.trim()) {
      setAccessTokenMissing(true);
      valid = false;
    } else {
      setAccessTokenMissing(false);
    }

    return valid;
  }

  async function handleLogin() {
    if (!validFormInput()) {
      return;
    }

    try {
      setIsSubmitting(true);
      const result = await getCompanyConfiguration(serviceAccount);
      saveIxonAuth(serviceAccount, result);
      setErrorMessage("");
      markStepCompleted("login");
      navigate(Pages.plcConnect);
    } catch (error) {
      clearIxonAuthenticationHeaders();
      setErrorMessage(error instanceof Error ? error.message : "Failed to login. Please check your credentials and try again.");
    } finally {
      setIsSubmitting(false);
    }
  }

  return (
    <WizardPage
      wizardStep="login"
      title="Log in"
      continueButtonText="Log in"
      onContinue={handleLogin}
      loading={isSubmitting}>

      <form className={styles.loginForm} noValidate>
        <div className={styles.formFieldWrapper}>
          <FormField
            id="applicationid"
            label="API Application ID"
            value={serviceAccount.apiApplicationID}
            onChange={(value) => setAuthProperty("apiApplicationID", value)}
            required
            invalidText={applicationIdMissing ? "API Application ID is required" : ""}
          />
          <FormField
            id="accesstoken"
            label="Access Token"
            type="password"
            value={serviceAccount.accessToken}
            onChange={(value) => setAuthProperty("accessToken", value)}
            required
            invalidText={accessTokenMissing ? "Access Token is required" : ""}
          />
          {errorMessage && <WarningTag invalidText={errorMessage} />}
        </div>
      </form>

      <VideoPreview onClick={() => setVideoOpen(true)} />

      <Modal isOpen={videoOpen} onClose={() => setVideoOpen(false)}>
        <Video onClose={() => setVideoOpen(false)} />
      </Modal>
    </WizardPage>
  )
}

export default LoginPage;