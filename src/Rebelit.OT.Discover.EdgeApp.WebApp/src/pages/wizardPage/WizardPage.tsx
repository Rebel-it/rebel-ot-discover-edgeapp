import Button from "../../components/atoms/button/Button";
import ProgressPanel from "../../components/organisms/ProgressPanel/ProgressPanel";
import type { WizardStepKey } from "../../models/WizardStep";
import styles from "./WizardPage.module.css";

type Props = {
  children: React.ReactNode;
  wizardStep: WizardStepKey;
  continueButtonText?: string;
  onContinue?: () => void;
}

export default function WizardPage({ children, wizardStep, continueButtonText, onContinue }: Readonly<Props>) {
  return (
    <div className={styles.wizardPage}>
      <div className={styles.content}>
        {children}
        <div className={styles.buttonWrapper}>
          {continueButtonText && onContinue && (
            <Button text={continueButtonText} onClick={onContinue} />
          )}
        </div>
      </div>
      <div className={styles.progressPanelWrapper}>
        <ProgressPanel currentStep={wizardStep} />
      </div>
    </div>
  );
}