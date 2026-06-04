import style from "./TableBody.module.css";
import TableRow from "./TableRow";
import { type ColumnDef, type RowData } from "./types";

export interface TableBodyProps {
  rows: RowData[];
  columns: ColumnDef[];
  selectedIds: string[];
  onRowSelect: (id: string) => void;
}

export default function TableBody({ rows, columns, selectedIds, onRowSelect }: Readonly<TableBodyProps>) {
  return (
    <div className={style.tableBody}>
      {rows.map((row) => (
        <TableRow
          key={row.id}
          row={row}
          columns={columns}
          selected={selectedIds.includes(row.id)}
          onSelect={onRowSelect}
        />
      ))}
    </div>
  );
}