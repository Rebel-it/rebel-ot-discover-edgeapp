import { useNavigate } from "react-router-dom";
import WizardPage from "../wizardPage/WizardPage";
import { Pages } from "../../models/Pages";
import WizardPageTitle from "../../components/atoms/wizardPageTitle/WizardPageTitle";

function FinalPage() {
  const navigate = useNavigate();

  return (
    <WizardPage
      wizardStep="final"
      continueButtonText="Finish"
      onContinue={() => {
        navigate(Pages.start);
      }}>
      <div>
        <WizardPageTitle title="Finish" />
      </div>
    </WizardPage>
  )
}

export default FinalPage;