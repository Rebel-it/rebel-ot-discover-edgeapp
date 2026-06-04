import styles from './Checkbox.module.css';

type Props = {
  label: string;
  checked: boolean;
  onChange: (checked: boolean) => void;
  orangeBorder?: boolean;
}

export default function Checkbox({ label, checked, onChange, orangeBorder }: Props) {
  return (
    <div className={`${styles.checkbox} ${orangeBorder ? styles.orangeBorder : ''}`}>
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