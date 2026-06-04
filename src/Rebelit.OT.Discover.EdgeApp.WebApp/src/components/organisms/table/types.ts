export type ColumnDef = {
  key: string;
  label: string;
  sortable?: boolean;
}

export type RowData = {
  id: string;
  cells: Record<string, string>;
}

export type CheckState = "none" | "some" | "all";
