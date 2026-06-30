import type { ComponentProps } from 'react'
import styles from './FormField.module.css'
import WarningTag from '../../atoms/warningTag/WarningTag';
import Tooltip from '../../atoms/tooltip/Tooltip';

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
  tooltip?: string;
}

function FormField({ id, label, value, onChange,
  type = 'text', required, placeholder, invalidText, prefix, tooltip }: Readonly<Props>) {
  return (
    <div className={styles.formField}>
      <div className={styles.labelRow}>
        <label htmlFor={id}>{label}</label>
        {tooltip && <Tooltip text={tooltip} />}
      </div>
      <div className={prefix ? `${styles.inputWrapper} ${styles.withPrefix}` : styles.inputWrapper}>
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