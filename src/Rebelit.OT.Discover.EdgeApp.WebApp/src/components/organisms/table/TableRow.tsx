
import CheckboxCell from "./cells/CheckboxCell";
import TextCell from "./cells/TextCell";
import style from "./TableBody.module.css";
import type { ColumnDef, RowData } from "./types";

export interface TableRowProps {
  row: RowData;
  columns: ColumnDef[];
  selected: boolean;
  onSelect: (id: string) => void;
}

export default function TableRow({ row, columns, selected, onSelect }: Readonly<TableRowProps>) {
  const rowClass = [style.row, selected ? style.selected : ""].filter(Boolean).join(" ");

  return (
    <div className={rowClass}>
      <CheckboxCell
        checked={selected}
        onChange={() => onSelect(row.id)}
        ariaLabel={`Select row ${row.id}`}
      />
      {columns.map((column) => {
        const cell = row.cells[column.key];
        if (!cell) {
          return null;
        }
        return <TextCell key={column.key} value={cell} />;
      })}
    </div>
  );
}