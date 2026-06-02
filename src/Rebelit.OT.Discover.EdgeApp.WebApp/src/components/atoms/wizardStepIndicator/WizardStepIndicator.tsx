import styles from './WizardStepIndicator.module.css';

type Props = {
  step: number;
}

export default function WizardStepIndicator({ step }: Readonly<Props>) {
  return (
    <div className={styles.wizardStepIndicator}>
      <span className={styles.content}>      
        {step}
      </span>
    </div>
  );
}