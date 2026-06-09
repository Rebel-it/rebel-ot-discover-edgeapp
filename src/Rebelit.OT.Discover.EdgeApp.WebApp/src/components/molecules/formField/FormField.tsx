import type { ComponentProps } from 'react'
import styles from './FormField.module.css'
import WarningTag from '../../atoms/warningTag/WarningTag';

type Props = {
  id: string;
  label: string;
  value: string;
  onChange: (value: string) => void;
  type?: ComponentProps<'input'>['type'];
  required?: boolean;
  placeholder?: string;
  invalidText?: string;
  prefix?: string;
}

function FormField({ id, label, value, onChange,
  type = 'text', required, placeholder, invalidText, prefix }: Readonly<Props>) {
  return (
    <div className={styles.formField}>
      <label htmlFor={id}>{label}</label>
      <div className={styles.inputWrapper}>
        {prefix && <span className={styles.prefix}>{prefix}</span>}
        <input
          id={id}
          type={type}
          value={value}
          inputMode={type === 'password' ? undefined : 'text'}
          onChange={(e) => onChange(e.target.value)}
          required={required}
          placeholder={placeholder}
        />
      </div>

      {invalidText && <WarningTag invalidText={invalidText} />}
    </div>
  )
}

export default FormField;