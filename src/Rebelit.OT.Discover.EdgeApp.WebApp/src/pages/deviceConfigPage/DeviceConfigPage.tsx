import { useState } from 'react'
import styles from './DeviceConfigPage.module.css'
import { PushDeviceConfiguration } from '../../services/IxonSettingService'
import { loadIxonAuthenticationHeaders } from '../../services/sessionStorageService'
import { useNavigate } from 'react-router-dom';
import { useWizard } from '../../context/WizardContext'
import WizardPage from '../wizardPage/WizardPage'
import { Pages } from '../../models/Pages'
import WizardPageTitle from '../../components/atoms/wizardPageTitle/WizardPageTitle'
import Table from '../../components/organisms/table/Table';

function DeviceConfigPage() {
  const navigate = useNavigate();
  const { markStepCompleted } = useWizard();
  const [loading, setLoading] = useState(false);
  const [status, setStatus] = useState<{ type: 'success' | 'error'; message: string } | null>(null);
  const [pushDeviceSuccess, setPushDeviceSuccess] = useState(false);

  async function handlePushConfiguration() {
    const auth = loadIxonAuthenticationHeaders();
    if (!auth?.AgentId) {
      setStatus({ type: 'error', message: 'No agent ID found. Please log in again.' });
      return;
    }

    setLoading(true);
    setStatus(null);

    try {
      await PushDeviceConfiguration(auth.AgentId);
      setStatus({ type: 'success', message: 'Configuration pushed successfully.' });
      setPushDeviceSuccess(true);
    } catch {
      setStatus({ type: 'error', message: 'Failed to push configuration. Please try again.' })
    } finally {
      setLoading(false);
    }
  }

  const columnDefs = [
    { key: "column1", label: "Step", sortable: true },
    { key: "column2", label: "Action", sortable: true },
    { key: "column3", label: "Estimated Time", sortable: true },
  ]

  const rowData = [
    {
      id: "1",
      cells: {
        column1: "Step 1",
        column2: "Connect to VPN",
        column3: "Estimated time: 2 minutes"
      }
    },
    {
      id: "2",
      cells: {
        column1: "Step 2",
        column2: "Connect to VPNNN",
        column3: "Estimated time: 4 minutes"
      }
    },
  ]

  return (
    <WizardPage
      wizardStep="deviceConfig"
      continueButtonText="Push to device"
      onContinue={() => {
        markStepCompleted("deviceConfig");
        navigate(Pages.final);
      }}>
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
          <p className={`${styles.formMessage} ${status.type === 'success' ? styles.successMessage : styles.errorMessage}`}>
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
