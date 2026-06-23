import styles from "./FinalPage.module.css";
import { useNavigate } from "react-router-dom";
import WizardPage from "../wizardPage/WizardPage";
import { Pages } from "../../models/Pages";
import ContactLabel from "../../components/atoms/contactLabel/ContactLabel";
import mailIcon from "../../assets/mailicon.svg";
import phoneIcon from "../../assets/phoneicon.svg";
import RichTextRenderer from "../../components/atoms/richTextRenderer/RichTextRenderer";
import { loadIxonAuthenticationHeaders } from "../../services/sessionStorageService";

function FinalPage() {
  const navigate = useNavigate();
  const auth = loadIxonAuthenticationHeaders();

  const variablesLink = auth?.AgentId && auth?.SourceId
    ? `https://portal.ixon.cloud/fleet-manager/device-configurator/${auth.AgentId}/services/data-source/${auth.SourceId}/variables`
    : "https://portal.ixon.cloud/fleet-manager/devices";

  const tagsLink = auth?.AgentId && auth?.SourceId
    ? `https://portal.ixon.cloud/fleet-manager/device-configurator/${auth.AgentId}/services/data-source/${auth.SourceId}/tags`
    : "https://portal.ixon.cloud/fleet-manager/devices";

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
            <li>
              <RichTextRenderer text={`Modify variables in the [Variables Overview in the Fleet Manager](${variablesLink})`} />
            </li>
            <li>
              <RichTextRenderer text={`Adjust tags on the [Tags page in the Fleet Manager](${tagsLink})`} />
            </li>
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