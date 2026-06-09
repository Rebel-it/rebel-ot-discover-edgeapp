import Sort from "../../atoms/sort/Sort";
import style from "./ColumnHeader.module.css";

export type ColumnHeaderProps = {
  label: string;
  onSort?: () => void;
  sortable?: boolean;
}

export default function ColumnHeader({ label, onSort, sortable = true }: Readonly<ColumnHeaderProps>) {
  return (
    <button
      className={`${style.columnHeader} ${sortable ? style.sortable : ''}`}
      onClick={sortable ? onSort : undefined}
    >
      <p>{label}</p>
      {sortable && <Sort width={16} height={16} />}
    </button>
  );
}
