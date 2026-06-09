import styles from './Button.module.css';

type Props = {
  text: string;
  onClick: () => void;
  loading?: boolean;
}

export default function Button({ text, onClick, loading }: Readonly<Props>) {
  return (
    <button className={styles.button} onClick={onClick} disabled={loading}>
      {text}
    </button>
  )
}