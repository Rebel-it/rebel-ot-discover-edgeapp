import styles from "./FinalPage.module.css";
import { useNavigate } from "react-router-dom";
import WizardPage from "../wizardPage/WizardPage";
import { Pages } from "../../models/Pages";
import ContactLabel from "../../components/atoms/contactLabel/ContactLabel";
import mailIcon from "../../assets/mailicon.svg";
import phoneIcon from "../../assets/phoneicon.svg";

function FinalPage() {
  const navigate = useNavigate();

  return (
    <WizardPage
      wizardStep="final"
      title="Finish"
      continueButtonText="Finish"
      onContinue={() => {
        navigate(Pages.start);
      }}>
      <div className={styles.page}>
        <>
          <p>You have successfully completed the setup! If needed, you can run the wizard again at any time to update your configuration.</p>
          <br />
          <p>Alternatively, you can:</p>
          <ul>
            <li>Modify variables in the Variables Overview in the Fleet Manager</li>
            <li>Adjust tags on the Tags page in the Fleet Manager</li>
          </ul>
          <br />
          <p>Finally, if you wish to remove the application, you can run the provided remove_discover_edgeapp.sh script to uninstall the DiscoverEdgeApp from your Secure Edge Pro.</p>
          <br />
          <br />
          <p>If you need any assistance or have questions beyond this setup, feel free to contact Rebel:it:</p>

          <div className={styles.contactDetailsWrapper}>
            <ContactLabel label="info@rebelit.nl" type="email">
              <img src={mailIcon} alt="Mail" />
            </ContactLabel>

            <ContactLabel label="085 06 04 167" type="phone">
              <img src={phoneIcon} alt="Phone" />
            </ContactLabel>
          </div>
        </>
      </div>
    </WizardPage>
  )
}

export default FinalPage;