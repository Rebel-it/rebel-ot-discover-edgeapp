import { useWizard } from "../../../context/WizardContext";
import { WizardStep, WizardStepOrder, type WizardStepKey } from "../../../models/WizardStep";
import WizardStepComponent from "../../molecules/wizardStepComponent/WizardStepComponent";
import styles from "./ProgressPanel.module.css";

type Props = {
  currentStep: WizardStepKey;
}

export default function ProgressPanel({ currentStep }: Readonly<Props>) {
  const { isStepCompleted } = useWizard();

  return (
    <div className={styles.progressPanel}>
      {WizardStepOrder.map((key, index) => {
        const step = WizardStep[key];
        const status = isStepCompleted(key) ? 'done'
          : key === currentStep ? 'active'
          : 'todo';

        return (
          <WizardStepComponent
            key={key}
            stepNumber={index + 1}
            wizardStep={step}
            status={status}
          />
        );
      })}
    </div>
  );
}