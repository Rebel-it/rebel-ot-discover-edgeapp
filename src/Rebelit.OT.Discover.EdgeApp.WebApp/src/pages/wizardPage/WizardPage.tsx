import Button from "../../components/atoms/button/Button";
import ProgressPanel from "../../components/organisms/progressPanel/ProgressPanel";
import type { WizardStepKey } from "../../models/WizardStep";
import styles from "./WizardPage.module.css";
import poweredByRebel from '../../assets/poweredbyrebel.png';

type Props = {
  children: React.ReactNode;
  title: string;
  wizardStep?: WizardStepKey;
  continueButtonText?: string;
  onContinue?: () => void;
  loading?: boolean;
}

export default function WizardPage({ children, title,
  wizardStep, continueButtonText,
  onContinue, loading }: Readonly<Props>) {
  return (
    <div className={styles.wizardPage}>
      <div className={styles.content}>
        <h1 className={styles.title}>{title}</h1>
        {children}
        <div className={styles.buttonWrapper}>
          {continueButtonText && onContinue && (
            <Button text={continueButtonText} onClick={onContinue} loading={loading} />
          )}
        </div>
      </div>
      <div className={styles.progressPanelWrapper}>
        <ProgressPanel currentStep={wizardStep} />
      </div>
      <img className={styles.poweredByRebel} src={poweredByRebel} alt="Powered by rebel:it" />
    </div>
  );
}