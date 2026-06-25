import style from "./Table.module.css";
import TableBody from "./TableBody";
import TableHeaderRow from "./TableHeaderRow";
import type { CheckState, ColumnDef, RowData } from "./types";

export interface TableProps {
    rows: RowData[];
    columns: ColumnDef[];
    onSort: (key: string) => void;
    selectedIds?: string[];
    onRowSelect?: (id: string) => void;
    onSelectAll?: () => void;
}

function getCheckState(rows: RowData[], selectedIds: string[]): CheckState {
    if (rows.length === 0 || selectedIds.length === 0) {
        return "none";
    }

    if (selectedIds.length === rows.length) {
        return "all";
    }

    return "some";
}

export default function Table({ rows, columns, selectedIds, onRowSelect, onSelectAll, onSort }: Readonly<TableProps>) {
    const checkState = selectedIds ? getCheckState(rows, selectedIds) : undefined;
    const hasCheckbox = checkState !== undefined;
    const gridTemplateColumns = [
        ...(hasCheckbox ? ['max-content'] : []),
        ...columns.map(col => `${col.width ?? 1}fr`),
    ].join(' ');

    return (
        <div className={style.table} style={{ '--gtc': gridTemplateColumns } as React.CSSProperties}>
            <TableHeaderRow
                columns={columns}
                checkState={checkState}
                onSelectAll={onSelectAll}
                onSort={onSort}
            />
            <TableBody
                rows={rows}
                columns={columns}
                selectedIds={selectedIds}
                onRowSelect={onRowSelect}
            />
        </div>
    );
}