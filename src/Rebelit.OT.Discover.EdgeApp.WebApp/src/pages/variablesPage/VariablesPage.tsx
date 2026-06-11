import { useEffect, useState } from "react"
import { useNavigate } from "react-router-dom"
import { synchronizeVariables as synchronizeVariablesRequest } from "../../services/scraperService"
import styles from "./VariablesPage.module.css"
import { useWizard } from "../../context/WizardContext"
import WizardPage from "../wizardPage/WizardPage"
import { Pages } from "../../models/Pages"
import Spinner from "../../components/atoms/spinner/Spinner"
import taskFinished from '../../assets/taskfinished.png';
import WarningTag from "../../components/atoms/warningTag/WarningTag"

function VariablesPage() {
  const navigate = useNavigate();
  const { markStepCompleted } = useWizard();
  const [isSubmitting, setIsSubmitting] = useState(true);
  const [errorMessage, setErrorMessage] = useState("");

  useEffect(() => {
    let isActive = true

    async function runSynchronization() {
      setErrorMessage("");

      try {
        await synchronizeVariablesRequest();

        if (!isActive) {
          return;
        }

      } catch (error) {
        if (!isActive) {
          return;
        }

        setErrorMessage(error instanceof Error ? error.message : "Variable synchronization failed. Please try again.");
      } finally {
        if (isActive) {
          setIsSubmitting(false);
        }
      }
    }

    void runSynchronization();

    return () => {
      isActive = false;
    }
  }, [])

  return (
    <WizardPage
      title="Synchronize variables"
      wizardStep="variables"
      continueButtonText="Continue"
      onContinue={() => {
        markStepCompleted("variables");
        navigate(Pages.tags);
      }}
      continueDisabled={isSubmitting}
    >
      <div className={styles.page}>
        <>
          <div className={styles.statusDescriptionWrapper}>
            {isSubmitting ? (
              <>
                <p>The following steps will now be performed:</p>
                <ol>
                  <li>Retrieving variables from your OPC UA Server</li>
                  <li>Synchronizing variables to the data source in the IXON Cloud</li>
                </ol>
                <br />
                <p>This may take a while</p>
              </>
            ) : (
              <p>Sync completed</p>
            )
            }
          </div>

          <div className={styles.statusIndicatorWrapper}>
            {isSubmitting ?
              (
                <Spinner />
              )
              : (
                <img src={taskFinished} alt="Variables sync completed" />
              )
            }
          </div>
        </>
        {errorMessage && <WarningTag invalidText={errorMessage} />}
      </div>
    </WizardPage>
  )
}

export default VariablesPage;