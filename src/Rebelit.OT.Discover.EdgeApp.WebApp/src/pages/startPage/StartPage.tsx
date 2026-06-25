import { useNavigate } from "react-router-dom";
import styles from "./StartPage.module.css";
import WizardPage from "../wizardPage/WizardPage";
import { Pages } from "../../models/Pages";
import { useEffect } from "react";
import { useWizard } from "../../context/WizardContext";
import RichTextRenderer from "../../components/atoms/richTextRenderer/RichTextRenderer";

function StartPage() {
  const navigate = useNavigate();
  const { deleteCompletedSteps } = useWizard();
  const description = "With the OPC UA Edge Sync, you can easily connect your machines to the cloud using your IXON Secure Edge Pro. This enables you to remotely monitor and keep full visibility over your operations.\n\nTo get started, we'll guide you through a simple setup wizard. In just 7 easy steps, you'll configure your IXON Secure Edge Pro. The process takes approximately 15 minutes.\n\n**Note:** You need to be connected to your IXON Secure Edge Pro via VPN.";

  useEffect(() => {
    deleteCompletedSteps();
  }, []);

  return (
    <WizardPage
      title=""
      continueButtonText="Start"
      onContinue={() => navigate(Pages.login)}
    >
      <section className={styles.page}>
        <h1 className={styles.title}>OPC UA Edge Sync</h1>
        <section className={styles.descriptionWrapper}>
          <RichTextRenderer text={description} />
        </section>
      </section>
    </WizardPage>
  )
}

export default StartPage;
