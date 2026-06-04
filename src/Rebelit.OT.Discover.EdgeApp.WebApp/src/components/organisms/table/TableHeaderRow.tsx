import { useRef, useEffect } from "react";
import style from "./TableHeaderRow.module.css";
import { type ColumnDef, type CheckState } from "./types";
import CheckboxCell from "./cells/CheckboxCell";
import ColumnHeader from "../../molecules/columnHeader/ColumnHeader";

export interface TableHeaderRowProps {
  columns: ColumnDef[];
  onSort: (key: string) => void;
  checkState?: CheckState;
  onSelectAll?: () => void;
}

export default function TableHeaderRow({ columns, checkState, onSelectAll, onSort }: Readonly<TableHeaderRowProps>) {
  const checkboxRef = useRef<HTMLInputElement>(null);

  useEffect(() => {
    if (checkboxRef.current) {
      checkboxRef.current.indeterminate = checkState === "some";
    }
  }, [checkState]);

  return (
    <div className={style.headerRow}>
      {checkState !== undefined && onSelectAll && (
        <CheckboxCell
          checked={checkState === "all"}
          onChange={onSelectAll}
          ariaLabel="Select all rows"
        />
      )}
      {columns.map((column) => (
        <ColumnHeader
          key={column.key}
          label={column.label}
          sortable={column.sortable}
          onSort={() => onSort(column.key)}
        />
      ))}
    </div>
  );
}