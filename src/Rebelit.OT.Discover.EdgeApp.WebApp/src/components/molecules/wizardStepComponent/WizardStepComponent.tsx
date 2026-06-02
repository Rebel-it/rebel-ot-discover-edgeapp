import type { WizardStep as step } from '../../../models/WizardStep';
import WizardStepIndicator from '../../atoms/wizardStepIndicator/WizardStepIndicator';
import styles from './WizardStepComponent.module.css';

type Props = {
  wizardStep: step;
  status: "todo" | "active" | "done";
}

export default function WizardStepComponent({ wizardStep, status }: Props) {
  return (
    <div className={styles.wizardStep}>
      <div className={styles.header}>
        <WizardStepIndicator step={1} status={status} />
        <h2>{wizardStep.title}</h2>
      </div>
      <div className={styles.description}>
        <p>{wizardStep.description}</p>
      </div>
    </div>
  );
}