import type { ComponentProps } from 'react'
import styles from './FormField.module.css'

type Props = {
  id: string
  label: string
  value: string
  onChange: (value: string) => void
  type?: ComponentProps<'input'>['type']
  required?: boolean
}

function FormField({ id, label, value, onChange,
  type = 'text', required }: Readonly<Props>) {
  return (
    <div className={styles.formField}>
      <label htmlFor={id}>{label}</label>
      <input
        id={id}
        type={type}
        value={value}
        inputMode={type === 'password' ? undefined : 'text'}
        onChange={(e) => onChange(e.target.value)}
        required={required}
      />
    </div>
  )
}

export default FormField;