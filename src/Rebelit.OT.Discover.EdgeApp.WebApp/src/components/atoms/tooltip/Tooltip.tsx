import styles from './Tooltip.module.css';
import tooltipIcon from '../../../assets/tooltip.svg';

type Props = {
  text: string;
}

export default function Tooltip({ text }: Readonly<Props>) {
  return (
    <div className={styles.tooltip}>
      <img src={tooltipIcon} alt="Info" className={styles.icon} />
      <div className={styles.popup} role="tooltip">
        <span>{text}</span>
        <div className={styles.arrow} />
      </div>
    </div>
  );
}
