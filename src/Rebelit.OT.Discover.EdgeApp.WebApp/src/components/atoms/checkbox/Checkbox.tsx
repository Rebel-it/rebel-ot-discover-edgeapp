import styles from './Checkbox.module.css';

type Props = {
  label: string;
  checked: boolean;
  onChange: (checked: boolean) => void;
}

export default function Checkbox({ label, checked, onChange }: Props) {
  return (
    <div className={styles.checkbox}>
      <label>
        <input
          type="checkbox"
          checked={checked}
          onChange={(e) => onChange(e.target.checked)}
        />
        {label}
      </label>
    </div>
  )
}