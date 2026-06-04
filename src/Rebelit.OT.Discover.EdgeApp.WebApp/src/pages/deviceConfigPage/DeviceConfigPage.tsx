import { useState } from "react"
import styles from "./DeviceConfigPage.module.css"
import { PushDeviceConfiguration } from "../../services/IxonSettingService"
import { loadIxonAuthenticationHeaders } from "../../services/sessionStorageService"
import { useNavigate } from "react-router-dom";
import { useWizard } from "../../context/WizardContext"
import WizardPage from "../wizardPage/WizardPage"
import { Pages } from "../../models/Pages"
import WizardPageTitle from "../../components/atoms/wizardPageTitle/WizardPageTitle"
import Table from "../../components/organisms/table/Table";
import { useTags } from "../../context/TagContext";
import { getFormulaLabel, getTagKey } from "../../models/Tag";

function DeviceConfigPage() {
  const navigate = useNavigate();
  const { tags } = useTags();
  const { markStepCompleted } = useWizard();
  const [loading, setLoading] = useState(false);
  const [status, setStatus] = useState<{ type: "success" | "error"; message: string } | null>(null);

  async function handlePushConfiguration() {
    const auth = loadIxonAuthenticationHeaders();
    if (!auth?.AgentId) {
      setStatus({ type: "error", message: "No agent ID found. Please log in again." });
      return;
    }

    setLoading(true);
    setStatus(null);

    try {
      await PushDeviceConfiguration(auth.AgentId);
      setStatus({ type: "success", message: "Configuration pushed successfully." });
      markStepCompleted("deviceConfig");
      navigate(Pages.final);
    } catch {
      setStatus({ type: "error", message: "Failed to push configuration. Please try again." })
    } finally {
      setLoading(false);
    }
  }

  const columnDefs = [
    { key: "name", label: "Name", sortable: true },
    { key: "logOn", label: "Log on", sortable: false },
    { key: "interval", label: "Interval", sortable: false },
    { key: "formula", label: "Formula", sortable: false },
  ]

  const rowData = tags.map((tag) => ({
    id: getTagKey(tag),
    cells: {
      name: tag.name,
      logOn: tag.logEvent,
      interval: tag.loggingInterval,
      formula: getFormulaLabel(tag)
    }
  }));

  return (
    <WizardPage
      wizardStep="deviceConfig"
      continueButtonText="Push to device"
      onContinue={handlePushConfiguration}
      loading={loading}>

      <div className={styles.page}>
        <WizardPageTitle title="Device Configuration" />
        <div className={styles.descriptionWrapper}>
          <p>
            The following data will be written to your edge router, there could be additional costs after doing this?
          </p>
          <p className={styles.warning}>
            Note: when you push this, everyone on the VPN will be disconnected
          </p>
        </div>

        {status && (
          <p className={`${styles.formMessage} ${status.type === "success" ? styles.successMessage : styles.errorMessage}`}>
            {status.message}
          </p>
        )}
        <div className={styles.tableWrapper}>
          <Table
            rows={rowData}
            columns={columnDefs}
            selectedIds={[]}
            onRowSelect={() => { }}
            onSelectAll={() => { }}
            onSort={() => { }}
          />
        </div>
      </div>
    </WizardPage>
  )
}

export default DeviceConfigPage;
