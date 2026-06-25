import styles from './Checkbox.module.css';

type Props = {
  label: string;
  checked: boolean;
  onChange: (checked: boolean) => void;
  theme: "purple" | "orange";
}

export default function Checkbox({ label, checked, onChange, theme }: Props) {
  return (
    <div className={`${styles.checkbox} ${styles[theme]}`}>
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