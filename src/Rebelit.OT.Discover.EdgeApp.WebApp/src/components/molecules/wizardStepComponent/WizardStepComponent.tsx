import type { WizardStep as step } from '../../../models/WizardStep';
import WizardStepIndicator from '../../atoms/wizardStepIndicator/WizardStepIndicator';
import styles from './WizardStepComponent.module.css';

type Props = {
  stepNumber: number;
  wizardStep: step;
  status: "todo" | "active" | "done";
  isCurrentStep: boolean;
}

export default function WizardStepComponent({
  stepNumber,
  wizardStep,
  status,
  isCurrentStep }: Props) {
  return (
    <div className={`${styles.wizardStep} ${styles[status]}`}>
      <div className={styles.header}>
        <WizardStepIndicator step={stepNumber} status={status} />
        <h2>{wizardStep.title}</h2>
      </div>
      <div className={styles.description}>
        {isCurrentStep && (
          <p>{wizardStep.description}</p>
        )}
      </div>
    </div>
  );
}