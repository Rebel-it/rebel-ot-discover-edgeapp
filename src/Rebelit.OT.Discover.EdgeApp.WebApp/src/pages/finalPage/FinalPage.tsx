import { useNavigate } from "react-router-dom";
import WizardPage from "../wizardPage/WizardPage";
import { Pages } from "../../models/Pages";

function FinalPage() {
  const navigate = useNavigate();

  return (
    <WizardPage
      wizardStep="final"
      continueButtonText="Finish"
      onContinue={() => {
        navigate(`/${Pages.start}`);
      }}>
      <div>
        <h1>Completed!</h1>
        <p>You have successfully completed the setup. If you wish, you can run the wizard again
          to make changes to your configuration.
          Finally, you can run the provided <code>remove_discover_edgeapp.sh</code> script to uninstall the DiscoverEdgeApp on your Secure Edge Pro.
        </p>
      </div>
    </WizardPage>
  )
}

export default FinalPage;