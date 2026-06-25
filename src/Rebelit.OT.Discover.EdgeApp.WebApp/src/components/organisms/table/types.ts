export type ColumnDef = {
  key: string;
  label: string;
  sortable?: boolean;
  width?: number; // Optional width in fraction units (e.g., 1, 2, 3)
}

export type RowData = {
  id: string;
  cells: Record<string, string>;
}

export type CheckState = "none" | "some" | "all";
