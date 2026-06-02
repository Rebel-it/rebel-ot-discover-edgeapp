import Button from "../../components/atoms/button/Button";
import ProgressPanel from "../../components/organisms/ProgressPanel/ProgressPanel";
import type { WizardStep } from "../../models/WizardStep";
import styles from "./WizardPage.module.css";

type Props = {
  children: React.ReactNode;
  wizardStep: WizardStep;
}

export default function WizardPage({ children, wizardStep }: Readonly<Props>) {
  return (
    <div className={styles.wizardPage}>
      <div>
        {children}
        <div className={styles.buttonWrapper}>
          <Button text="Back" onClick={() => window.history.back()} />
        </div>
      </div>
      <ProgressPanel wizardStep={wizardStep} />
    </div>
  );
}