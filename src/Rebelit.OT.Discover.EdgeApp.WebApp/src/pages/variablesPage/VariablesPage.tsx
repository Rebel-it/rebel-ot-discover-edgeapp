import { useEffect, useState } from "react"
import { useNavigate } from "react-router-dom"
import { synchronizeVariables as synchronizeVariablesRequest } from "../../services/scraperService"
import styles from "./VariablesPage.module.css"
import { useWizard } from "../../context/WizardContext"
import WizardPage from "../wizardPage/WizardPage"
import { Pages } from "../../models/Pages"
import WizardPageTitle from "../../components/atoms/wizardPageTitle/WizardPageTitle"
import Spinner from "../../components/atoms/spinner/Spinner"

function VariablesPage() {
  const navigate = useNavigate();
  const { markStepCompleted } = useWizard();
  const [isSubmitting, setIsSubmitting] = useState(true);
  const [synchronizationSucceeded, setSynchronizationSucceeded] = useState(false);
  const [errorMessage, setErrorMessage] = useState("");

  // useEffect(() => {
  //   let isActive = true

  //   async function runSynchronization() {
  //     setIsSubmitting(true);
  //     setErrorMessage("");
  //     setSynchronizationSucceeded(false);

  //     try {
  //       await synchronizeVariablesRequest();

  //       if (!isActive) {
  //         return;
  //       }

  //       setSynchronizationSucceeded(true);
  //     } catch (error) {
  //       if (!isActive) {
  //         return;
  //       }

  //       setErrorMessage(error instanceof Error ? error.message : "Variable synchronization failed. Please try again.");
  //     } finally {
  //       if (isActive) {
  //         setIsSubmitting(false);
  //       }
  //     }
  //   }

  //   void runSynchronization();

  //   return () => {
  //     isActive = false;
  //   }
  // }, [])

  return (
    <WizardPage
      wizardStep="variables"
      continueButtonText="Continue"
      onContinue={() => {
        markStepCompleted("variables");
        navigate(Pages.tags);
      }}>
      <div className={styles.page}>
        <WizardPageTitle title="Synchronize variables" />

        {isSubmitting && (
          <>
            <div className={styles.statusDescriptionWrapper}>
              <p>The following steps will now be performed:</p>
              <ol>
                <li>Retrieving variables from your machine</li>
                <li>Sending data to the IXON Cloud</li>
              </ol>
            </div>
            <div className={styles.spinnerWrapper}>
              <Spinner />
            </div>
          </>
        )}

        {errorMessage && <p className={`${styles.formMessage} ${styles.errorMessage}`}>{errorMessage}</p>}

        {synchronizationSucceeded && (
          <p className={`${styles.formMessage} ${styles.successMessage}`}>
            Variable synchronization succeeded. Continue to the next step.
          </p>
        )}

      </div>
    </WizardPage>
  )
}

export default VariablesPage;