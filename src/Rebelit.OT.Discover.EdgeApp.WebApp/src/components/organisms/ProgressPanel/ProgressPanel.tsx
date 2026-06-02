import type { WizardStep } from "../../../models/WizardStep";
import WizardStepComponent from "../../molecules/wizardStepComponent/WizardStepComponent";
import styles from "./ProgressPanel.module.css";

type Props = {
  wizardStep: WizardStep;
}

export default function ProgressPanel({ wizardStep }: Readonly<Props>) {
  return (
    <div className={styles.progressPanel}>
      <WizardStepComponent wizardStep={wizardStep} status="done" />
    </div>
  );
}