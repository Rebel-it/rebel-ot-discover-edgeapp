import styles from "./FinalPage.module.css";
import { useNavigate } from "react-router-dom";
import WizardPage from "../wizardPage/WizardPage";
import { Pages } from "../../models/Pages";
import WizardPageTitle from "../../components/atoms/wizardPageTitle/WizardPageTitle";
import SelectedTagsTable from "../../components/organisms/selectedTagsTable/SelectedTagsTable";

function FinalPage() {
  const navigate = useNavigate();

  return (
    <WizardPage
      wizardStep="final"
      continueButtonText="Finish"
      onContinue={() => {
        navigate(Pages.start);
      }}>
      <div className={styles.page}>
        <WizardPageTitle title="Finish" />
        <SelectedTagsTable />
      </div>
    </WizardPage>
  )
}

export default FinalPage;