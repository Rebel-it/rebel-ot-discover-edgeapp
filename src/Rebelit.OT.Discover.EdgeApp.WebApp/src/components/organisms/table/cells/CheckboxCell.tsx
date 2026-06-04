import Checkbox from "../../../atoms/checkbox/Checkbox";
import Cell from "./Cell";
import style from "./Cell.module.css";

export type CheckboxCellProps = {
  checked: boolean;
  onChange: (checked: boolean) => void;
  disabled?: boolean;
  ariaLabel?: string;
};

export default function CheckboxCell({ checked, onChange }: Readonly<CheckboxCellProps>) {
  return (
    <Cell className={style.narrow}>
      <Checkbox
        label=""
        checked={checked}
        onChange={onChange}
        orangeBorder={true}
      />
    </Cell>
  );
}
