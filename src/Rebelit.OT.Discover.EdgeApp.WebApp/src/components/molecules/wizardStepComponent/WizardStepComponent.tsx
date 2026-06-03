import type { WizardStep as step } from '../../../models/WizardStep';
import WizardStepIndicator from '../../atoms/wizardStepIndicator/WizardStepIndicator';
import styles from './WizardStepComponent.module.css';

type Props = {
  stepNumber: number;
  wizardStep: step;
  status: "todo" | "active" | "done";
}

export default function WizardStepComponent({ stepNumber, wizardStep, status }: Props) {
  return (
    <div className={styles.wizardStep}>
      <div className={styles.header}>
        <WizardStepIndicator step={stepNumber} status={status} />
        <h2>{wizardStep.title}</h2>
      </div>
      <div className={styles.description}>
        {status === "active" && (
          <p>{wizardStep.description}</p>
        )}
      </div>
    </div>
  );
}