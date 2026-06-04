import Cell from "./Cell";

export type TextCellProps = {
    value: string;
};

export default function TextCell({ value }: Readonly<TextCellProps>) {
    return (
        <Cell>
            {value}
        </Cell>
    );
}
