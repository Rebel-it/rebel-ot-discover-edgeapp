import Cell from "./Cell";

export type TextCellProps = {
    value: string;
    selected?: boolean;
};

export default function TextCell({ value, selected }: Readonly<TextCellProps>) {
    return (
        <Cell selected={selected}>
            <p>{value}</p>
        </Cell>
    );
}
