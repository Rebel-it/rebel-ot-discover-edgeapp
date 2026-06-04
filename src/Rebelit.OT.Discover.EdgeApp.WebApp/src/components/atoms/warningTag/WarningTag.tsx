import styles from './WarningTag.module.css';
import warningIcon from '../../../assets/warningicon.svg';

type Props = {
  invalidText: string;
}

export default function WarningTag({ invalidText }: Readonly<Props>) {
  return (
    <div className={styles.warningTag}>
      <img src={warningIcon} alt="Warning" />
      <p>
        {invalidText}
      </p>
    </div>
  );
}