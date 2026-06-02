import checkmark from '../../../assets/checkmark.svg';
import styles from './WizardStepIndicator.module.css';

type Props = {
  step: number;
  status: "todo" | "active" | "done";
}

export default function WizardStepIndicator({ step, status }: Readonly<Props>) {
  const statusClass = styles[status] ?? '';
  const indicatorStyle = `${styles.wizardStepIndicator} ${statusClass}`;
  const contentStyle = `${styles.content} ${statusClass}`;

  return (
    <div className={indicatorStyle}>
      <span className={contentStyle}>
        {status === "done" ? <img src={checkmark} alt="Completed" /> : step}
      </span>
    </div>
  );
}