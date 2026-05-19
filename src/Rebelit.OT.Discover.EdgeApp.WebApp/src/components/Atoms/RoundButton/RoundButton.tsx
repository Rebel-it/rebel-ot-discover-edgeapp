import type { ComponentProps } from 'react'
import styles from './RoundButton.module.css'

type buttonProps = {
    onClick?: () => void
    disabled?: boolean
    children: React.ReactNode
    type?: ComponentProps<'button'>['type']
}

function RoundButton({ onClick, disabled = false, children, type = 'button' }: Readonly<buttonProps>) {
    return (
        <button type={type} className={styles.RoundButton} onClick={onClick} disabled={disabled}>
            {children}
        </button>
    )
}

export default RoundButton;