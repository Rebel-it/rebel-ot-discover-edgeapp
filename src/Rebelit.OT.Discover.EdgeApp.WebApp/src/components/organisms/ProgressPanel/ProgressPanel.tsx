import type { WizardStep } from "../../../models/WizardStep";
import WizardStepIndicator from "../../atoms/wizardStepIndicator/WizardStepIndicator";
import styles from "./ProgressPanel.module.css";

type Props = {
  wizardStep: WizardStep;
}

export default function ProgressPanel({ wizardStep }: Readonly<Props>) {
  return (
    <div className={styles.progressPanel}>
      <h2>{wizardStep.Title}</h2>
      <p>{wizardStep.Description}</p>
      <WizardStepIndicator step={1} status="done"/>
    </div>
  );
}