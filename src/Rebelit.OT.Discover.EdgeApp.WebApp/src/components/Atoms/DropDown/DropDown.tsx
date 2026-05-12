import styles from './DropDown.module.css'

type DropDownOption = {
	label: string
	value: string
}

type DropDownProps = {
	id: string
	label: string
	value: string
	options: DropDownOption[]
	onChange: (value: string) => void
	placeholder?: string
	disabled?: boolean
}

function DropDown({
	id,
	label,
	value,
	options,
	onChange,
	placeholder = 'Select an option',
	disabled = false,
}: Readonly<DropDownProps>) {
	return (
		<div className={styles.formField}>
			<label htmlFor={id}>{label}</label>
			<select
				id={id}
				value={value}
				onChange={(e) => onChange(e.target.value)}
				disabled={disabled}
			>
				<option value="">{placeholder}</option>
				{options.map((option) => (
					<option key={option.value} value={option.value}>
						{option.label}
					</option>
				))}
			</select>
		</div>
	)
}

export type { DropDownOption, DropDownProps }
export default DropDown
