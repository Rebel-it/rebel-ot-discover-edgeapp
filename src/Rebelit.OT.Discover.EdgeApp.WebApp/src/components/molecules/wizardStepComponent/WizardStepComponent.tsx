import { useEffect, useRef } from 'react';
import type { WizardStep as step } from '../../../models/WizardStep';
import WizardStepIndicator from '../../atoms/wizardStepIndicator/WizardStepIndicator';
import styles from './WizardStepComponent.module.css';
import RichTextRenderer from '../../atoms/richTextRenderer/RichTextRenderer';

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
  const ref = useRef<HTMLDivElement>(null);

  useEffect(() => {
    if (isCurrentStep) {
      ref.current?.scrollIntoView({ behavior: 'smooth', block: 'nearest' });
    }
  }, [isCurrentStep]);

  return (
    <div ref={ref} className={`${styles.wizardStep} ${styles[status]}`}>
      <div className={styles.header}>
        <WizardStepIndicator step={stepNumber} status={status} />
        <h2>{wizardStep.title}</h2>
      </div>
      <div className={styles.description}>
        {isCurrentStep && (
          <RichTextRenderer text={wizardStep.description} />
        )}
      </div>
    </div>
  );
}