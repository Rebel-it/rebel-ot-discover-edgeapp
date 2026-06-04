import { useNavigate } from "react-router-dom"
import styles from "./StartPage.module.css"
import WizardPage from "../wizardPage/WizardPage"
import { Pages } from "../../models/Pages"
import { useEffect } from "react"
import { useWizard } from "../../context/WizardContext"
import Table from "../../components/organisms/table/Table"

function StartPage() {
  const navigate = useNavigate();
  const { deleteCompletedSteps } = useWizard();
  const description = "With the [OPC & Discovery App], you can easily connect your machines to the cloud using your IXON device. This enables you to remotely monitor your factory and keep full visibility over your operations.\n\nTo get started, we’ll guide you through a simple setup wizard. In just 7 steps, you’ll configure your IXON device. The process takes approximately [X] minutes.\n\nNote: you need to be connected to your machine via VPN.";

  useEffect(() => {
    deleteCompletedSteps();
  }, []);

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
      continueButtonText="Start"
      onContinue={() => navigate(Pages.login)}
    >
      <section className={styles.page}>
        <h1 className={styles.title}>[OPC & Discovery app]</h1>
        <p className={styles.description}>
          {description.split("\n\n").map((paragraph, index) => (
            <span key={index}>
              {paragraph}
              <br /><br />
            </span>
          ))}
        </p>

        <Table
          rows={rowData}
          columns={columnDefs}
          selectedIds={[]}
          onRowSelect={() => { }}
          onSelectAll={() => { }}
          onSort={() => { }}
        />

      </section>
    </WizardPage>
  )
}

export default StartPage;
