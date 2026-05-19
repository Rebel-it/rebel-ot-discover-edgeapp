import type { ComponentProps } from 'react'
import styles from './FormField.module.css'
type FormFieldProps = {
    id: string
    label: string
    value: string
    onChange: (value: string) => void
    type?: ComponentProps<'input'>['type']
    required?: boolean
    autoComplete?: ComponentProps<'input'>['autoComplete']
}

function FormField({ id, label, value, onChange, type = 'text', required, autoComplete = 'off' }: Readonly<FormFieldProps>) {
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
                autoComplete={autoComplete}
            />
        </div>
    )
}

export default FormField
