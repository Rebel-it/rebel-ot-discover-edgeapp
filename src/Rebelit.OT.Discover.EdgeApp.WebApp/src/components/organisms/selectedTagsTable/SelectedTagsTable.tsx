import styles from "./SelectedTagsTable.module.css";
import { useTags } from "../../../context/TagContext";
import { getFormulaLabel, getTagKey } from "../../../models/Tag";
import Table from "../table/Table";

export default function SelectedTagsTable() {
  const { tags } = useTags();

  const columnDefs = [
    { key: "name", label: "Name", sortable: true },
    { key: "logOn", label: "Log on", sortable: false },
    { key: "interval", label: "Interval", sortable: false },
    { key: "formula", label: "Formula", sortable: false },
  ];

  const rowData = tags.map((tag) => ({
    id: getTagKey(tag),
    cells: {
      name: tag.name,
      logOn: tag.logEvent,
      interval: tag.loggingInterval,
      formula: getFormulaLabel(tag)
    }
  }));

  return (
    <div className={styles.tableWrapper}>
      <Table
        rows={rowData}
        columns={columnDefs}
        onSort={() => { }}
      />
    </div>
  );
}